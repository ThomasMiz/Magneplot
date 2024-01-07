#version 330

uniform mat4 World, View, Projection;

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoords;

out vec3 fPosition;
out vec3 fNormal;
out vec2 fTexCoords;

void main() {
	vec4 worldPos = World * vec4(vPosition, 1.0);
	gl_Position = Projection * View * worldPos;

	fPosition = worldPos.xyz;
	fNormal = (World * vec4(vNormal, 0.0)).xyz;
	fTexCoords = vTexCoords;
}