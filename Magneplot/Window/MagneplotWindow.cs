using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Maths;
using TrippyGL;

namespace Magneplot.Window
{
    class MagneplotWindow : WindowBase
    {
        const float CameraMoveSpeed = 2.5f;
        const float CameraMoveSpeedFast = CameraMoveSpeed * 5;
        VertexNormalTexture[] model;
        VertexPosition[] curve;
        double netFlow;
        float maxColorRadius;

        InputManager3D inputManager;

        SimpleShaderProgram linesProgram;
        VertexBuffer<VertexColor> linesBuffer;
        VertexBuffer<VertexColor> crossBuffer;

        ShaderProgram modelProgram;
        VertexBuffer<VertexNormalTexture> modelBuffer;

        SimpleShaderProgram curveProgram;
        VertexBuffer<VertexPosition> curveBuffer;


        public MagneplotWindow(double netFlow, VertexNormalTexture[] model, VertexPosition[] curve) : base(null, 24)
        {
            this.model = model;
            this.curve = curve;
            this.netFlow = netFlow;
            maxColorRadius = model.Select(v => MathF.Abs(v.TexCoords.X)).Max();
        }

        protected override void OnLoad()
        {
            inputManager = new InputManager3D(InputContext)
            {
                CameraMoveSpeed = 2.5f,
                CameraPosition = new Vector3(3, 6, 6),
                CameraRotationX = -0.5f,
                CameraRotationY = -2.0f,
            };

            Span<VertexColor> crossLines = stackalloc VertexColor[]
            {
                new VertexColor(new Vector3(0, 0, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 1, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(1, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(0, 0, 0), Color4b.Blue),
                new VertexColor(new Vector3(0, 0, 1), Color4b.Blue),
            };

            crossBuffer = new VertexBuffer<VertexColor>(graphicsDevice, crossLines, BufferUsage.StaticDraw);

            Span<VertexPosition> cube = stackalloc VertexPosition[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, 0.5f),
            };

            VertexColor[] linesArray = CreateLines();
            linesBuffer = new VertexBuffer<VertexColor>(graphicsDevice, linesArray, BufferUsage.StaticDraw);

            linesProgram = SimpleShaderProgram.Create<VertexColor>(graphicsDevice);

            curveBuffer = new VertexBuffer<VertexPosition>(graphicsDevice, curve, BufferUsage.StaticDraw);
            modelBuffer = new VertexBuffer<VertexNormalTexture>(graphicsDevice, model, BufferUsage.StaticDraw);
            curve = null;
            model = null;

            modelProgram = ShaderProgram.FromFiles<VertexNormalTexture>(graphicsDevice, "data/model.vs", "data/model.fs", "vPosition", "vNormal", "vTexCoords");
            modelProgram.Uniforms["modelAlpha"].SetValueFloat(0.5f);
            modelProgram.Uniforms["World"].SetValueMat4(Matrix4x4.CreateTranslation(0.1f, 0.1f, 0.1f));
            modelProgram.Uniforms["ambientLightColor"].SetValueVec3(Vector3.One);

            // To re-enable the fragment shader's lighting, uncomment this, the code calling calcDirLight in the shader code (data/model.fs:48)
            // and uncomment the code setting the cameraPos shader in the middle of the OnRender function.
            /*DirectionalLight dl = new DirectionalLight(modelProgram.Uniforms["dLightDir0"], modelProgram.Uniforms["dLightDiffColor0"], modelProgram.Uniforms["dLightSpecColor0"]);
            dl.Direction = new Vector3(1, -1, 1);
            modelProgram.Uniforms["reflectivity"].SetValueFloat(1);
            modelProgram.Uniforms["specularPower"].SetValueFloat(1);*/
            modelProgram.Uniforms["positiveColor"].SetValueVec3(1, 0, 0);
            modelProgram.Uniforms["negativeColor"].SetValueVec3(0, 1, 0);
            modelProgram.Uniforms["minMaxFlow"].SetValueVec2(-maxColorRadius, maxColorRadius);
            modelProgram.Uniforms["modelAlpha"].SetValueFloat(0.8f);

            SimpleShaderProgramBuilder builder = new();
            builder.ConfigureVertexAttribs<VertexPosition>();
            curveProgram = builder.Create(graphicsDevice);
            curveProgram.World = Matrix4x4.CreateTranslation(0.1f, 0.1f, 0.1f);

            graphicsDevice.ClearColor = Color4b.White;
            graphicsDevice.DepthState = DepthState.Default;
            graphicsDevice.BlendState = BlendState.Opaque;
        }

        protected override void OnRender(double dt)
        {
            bool fastCamera = inputManager.CurrentKeyboard?.IsKeyPressed(Key.ShiftLeft) == true
                || inputManager.CurrentKeyboard?.IsKeyPressed(Key.ShiftRight) == true
                || inputManager.CurrentGamepad?.LeftBumper().Pressed == true
                || inputManager.CurrentGamepad?.RightBumper().Pressed == true;
            inputManager.CameraMoveSpeed = fastCamera ? CameraMoveSpeedFast : CameraMoveSpeed;

            inputManager.Update((float)dt);
            Window.Title = string.Format("colorRadius={0} | netFlow={1} | {2}", maxColorRadius, netFlow, inputManager.CameraPosition);

            graphicsDevice.Clear(ClearBuffers.Color | ClearBuffers.Depth);

            Matrix4x4 view = inputManager.CalculateViewMatrix();

            graphicsDevice.DepthTestingEnabled = true;
            graphicsDevice.VertexArray = linesBuffer;
            graphicsDevice.ShaderProgram = linesProgram;
            linesProgram.World = Matrix4x4.Identity;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, linesBuffer.StorageLength);

            graphicsDevice.VertexArray = curveBuffer;
            graphicsDevice.ShaderProgram = curveProgram;
            curveProgram.View = view;
            curveProgram.Color = Color4b.Black;
            graphicsDevice.DrawArrays(PrimitiveType.LineStrip, 0, curveBuffer.StorageLength);

            graphicsDevice.VertexArray = modelBuffer;
            graphicsDevice.ShaderProgram = modelProgram;
            modelProgram.Uniforms["View"].SetValueMat4(view);
            //modelProgram.Uniforms["cameraPos"].SetValueVec3(inputManager.CameraPosition);
            graphicsDevice.DrawArrays(PrimitiveType.Triangles, 0, modelBuffer.StorageLength);

            graphicsDevice.DepthTestingEnabled = false;
            graphicsDevice.ShaderProgram = linesProgram;
            graphicsDevice.VertexArray = crossBuffer;
            Vector3 translation = inputManager.CameraPosition + inputManager.CalculateForwardVector();
            linesProgram.World = Matrix4x4.CreateScale(0.05f) * Matrix4x4.CreateTranslation(translation);
            linesProgram.View = view;
            graphicsDevice.DrawArrays(PrimitiveType.Lines, 0, crossBuffer.StorageLength);
        }

        protected override void OnResized(Vector2D<int> size)
        {
            if (size.X == 0 || size.Y == 0)
                return;

            graphicsDevice.SetViewport(0, 0, (uint)size.X, (uint)size.Y);
            Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2f, size.X / (float)size.Y, 0.1f, 50f);
            linesProgram.Projection = proj;
            modelProgram.Uniforms["Projection"].SetValueMat4(proj);
            curveProgram.Projection = proj;
        }

        protected override void OnUnload()
        {
            crossBuffer.Dispose();
            linesBuffer.Dispose();
            linesProgram.Dispose();
            modelBuffer.Dispose();
            modelProgram.Dispose();
            curveBuffer.Dispose();
            curveProgram.Dispose();
        }

        private static VertexColor[] CreateLines()
        {
            const float d = 100;

            List<VertexColor> lines = new List<VertexColor>
            {
                new VertexColor(new Vector3(-d, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(d, 0, 0), Color4b.Red),
                new VertexColor(new Vector3(0, -d, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, d, 0), Color4b.Lime),
                new VertexColor(new Vector3(0, 0, -d), Color4b.Blue),
                new VertexColor(new Vector3(0, 0, d), Color4b.Blue)
            };

            const byte secondaryLineAlpha = 64;
            Color4b darkRed = new Color4b(255, 0, 0, secondaryLineAlpha);
            Color4b darkGreen = new Color4b(0, 255, 0, secondaryLineAlpha);
            Color4b darkBlue = new Color4b(0, 0, 255, secondaryLineAlpha);

            for (int i = 1; i < 5; i++)
            {
                lines.Add(new VertexColor(new Vector3(-d, 0, i), darkRed));
                lines.Add(new VertexColor(new Vector3(d, 0, i), darkRed));
                lines.Add(new VertexColor(new Vector3(-d, 0, -i), darkRed));
                lines.Add(new VertexColor(new Vector3(d, 0, -i), darkRed));

                lines.Add(new VertexColor(new Vector3(i, -d, 0), darkGreen));
                lines.Add(new VertexColor(new Vector3(i, d, 0), darkGreen));
                lines.Add(new VertexColor(new Vector3(-i, -d, 0), darkGreen));
                lines.Add(new VertexColor(new Vector3(-i, d, 0), darkGreen));

                lines.Add(new VertexColor(new Vector3(i, 0, -d), darkBlue));
                lines.Add(new VertexColor(new Vector3(i, 0, d), darkBlue));
                lines.Add(new VertexColor(new Vector3(-i, 0, -d), darkBlue));
                lines.Add(new VertexColor(new Vector3(-i, 0, d), darkBlue));
            }

            return lines.ToArray();
        }
    }
}
