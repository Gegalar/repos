#version 330 core
out vec4 FragColor;

in VS_OUT {
	vec3 FragPos;
    vec2 TexCoords;
    vec3 TangentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform float heightScale;

vec2 PRM(vec2 inTexCoords, vec3 inViewDir, out float lastDepthValue)
{
   const float _minLayers = 8.0f;
   const float _maxLayers = 32.0f;
   float _numLayers = mix(_maxLayers, _minLayers, abs(dot(vec3(0.0f, 0.0f, 1.0f), inViewDir)));
   float deltaDepth = 1.0f/_numLayers;
	vec2 P = inViewDir.xy/ inViewDir.z * heightScale;
   vec2 deltaTexcoord = P / _numLayers;
   vec2 currentTexCoords = inTexCoords;
   float currentLayerDepth = 0.0f;
   float currentDepthValue = texture(depthMap, currentTexCoords).r;

   while (currentDepthValue > currentLayerDepth)
   {
       currentLayerDepth += deltaDepth;
       currentTexCoords -= deltaTexcoord;
       currentDepthValue = texture(depthMap, currentTexCoords).r;
   }

   deltaTexcoord *= 0.5;
   deltaDepth *= 0.5;
   currentTexCoords += deltaTexcoord;
   currentLayerDepth -= deltaDepth;
   const int _reliefSteps = 5;
   int currentStep = _reliefSteps;
   while (currentStep > 0)
   {
       currentDepthValue = texture(depthMap, currentTexCoords).r;
       deltaTexcoord *= 0.5;
       deltaDepth *= 0.5;
       if (currentDepthValue > currentLayerDepth)
       {
           currentTexCoords -= deltaTexcoord;
           currentLayerDepth += deltaDepth;
       }
       else
       {
           currentTexCoords += deltaTexcoord;
           currentLayerDepth -= deltaDepth;
       }
       currentStep--;
   }
   lastDepthValue = currentDepthValue;
   return currentTexCoords;
};

void main()
{
   /*float lastDepthValue;
   vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
   vec2 texCoords = PRM(fs_in.TexCoords, viewDir, lastDepthValue);

   vec3 normal = texture(normalMap, texCoords).rgb;
   normal = normalize(normal * 2.0f - 1.0f);

   vec3 ambient = 0.1 * texture(diffuseMap, fs_in.TexCoords).rgb;

   vec3 lightDir = normalize(-fs_in.TangentLightPos);
   float diff = max(dot(normal, lightDir), 0.0);
   vec3 diffuse = diff * texture(diffuseMap, fs_in.TexCoords).rgb;

   vec3 reflectDir = reflect(-lightDir, normal);
	// -lightDir, так как reflect должен принять
	// вектор, имеющий направление от источника света к объекту
   float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);

   vec3 specular = vec3(0.2) * spec;

   FragColor = vec4(ambient + diffuse + specular, 1.0f);*/
	// смещение текстурных координат при использовании Параллакс Отображения
    vec3 viewDir = normalize(fs_in.TangentViewPos - fs_in.TangentFragPos);
    vec2 texCoords = fs_in.TexCoords;
    float lastDepthValue;

    texCoords = PRM(fs_in.TexCoords, viewDir, lastDepthValue);      
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard;

    // получение нормали из карты нормалей
    vec3 normal = texture(normalMap, texCoords).rgb;
    normal = normalize(normal * 2.0 - 1.0);   
   
    // получение диффузного цвета
    vec3 color = texture(diffuseMap, texCoords).rgb;
    // фоновая составляющая
    vec3 ambient = 0.1 * color;
    // диффузная составляющая
    vec3 lightDir = normalize(fs_in.TangentLightPos - fs_in.TangentFragPos);
    float diff = max(dot(lightDir, normal), 0.0);
    vec3 diffuse = diff * color;
    // отраженная составляющая    
    vec3 reflectDir = reflect(-lightDir, normal);
    vec3 halfwayDir = normalize(lightDir + viewDir);  
    float spec = pow(max(dot(normal, halfwayDir), 0.0), 32.0);

    vec3 specular = vec3(0.2) * spec;
    FragColor = vec4(ambient + diffuse + specular, 1.0);
}


