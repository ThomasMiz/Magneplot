#version 330

uniform vec3 cameraPos;
uniform vec3 ambientLightColor;
uniform float reflectivity;
uniform float specularPower;
uniform vec3 dLightDir0;
uniform vec3 dLightDiffColor0;
uniform vec3 dLightSpecColor0;
uniform vec3 pLightPos0;
uniform vec3 pLightDiffColor0;
uniform vec3 pLightSpecColor0;
uniform vec3 pAttConfig0;

uniform vec3 positiveColor;
uniform vec3 negativeColor;
uniform vec2 minMaxFlow;

uniform float modelAlpha;

in vec3 fPosition;
in vec3 fNormal;
in vec2 fTexCoords;

out vec4 FragColor;

vec3 calcDirLight(in vec3 norm, in vec3 toCamVec, in vec3 lDir, in vec3 diffCol, in vec3 specCol) {
    float brightness = max(0.0, dot(norm, -lDir));
    vec3 reflectedDir = reflect(lDir, norm);
    float specFactor = max(0.0, dot(reflectedDir, toCamVec));
    float dampedFactor = pow(specFactor, specularPower);
    return brightness * diffCol + (dampedFactor * reflectivity) * specCol;
}

vec3 calcPosLight(in vec3 norm, in vec3 toCamVec, in vec3 lPos, in vec3 diffCol, in vec3 specCol, in vec3 attConfig) {
    float d = distance(fPosition, lPos);
    vec3 dirLight = calcDirLight(norm, toCamVec, normalize(fPosition - lPos), diffCol, specCol);
    return dirLight * clamp(1.0 / (attConfig.x + attConfig.y*d + attConfig.z*d*d), 0.0, 1.0);
}

void main() {
    float mixval = clamp((fTexCoords.x - minMaxFlow.x) / (minMaxFlow.y - minMaxFlow.x), -10, 10);
    vec4 finalColor = vec4(mix(negativeColor, positiveColor, mixval), modelAlpha);
    vec3 unitNormal = normalize(fNormal);
    vec3 unitToCameraVec = normalize(cameraPos - fPosition);

    vec3 light = ambientLightColor;
    //light += calcDirLight(unitNormal, unitToCameraVec, dLightDir0, dLightDiffColor0, dLightSpecColor0);
    //light += calcPosLight(unitNormal, unitToCameraVec, pLightPos0, pLightDiffColor0, pLightSpecColor0, pAttConfig0);

    finalColor.xyz *= light;
    FragColor = finalColor;
}