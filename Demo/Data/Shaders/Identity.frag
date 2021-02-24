#version 420

layout(binding = 0) uniform sampler2D textureSampler;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{
    out_Color = texture(textureSampler, fragUV);
}
