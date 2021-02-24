#version 420

layout(binding = 0) uniform sampler2D textureSampler;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{
    vec4 texColor = texture(textureSampler, fragUV);
    texColor.a *= 0.5;
    texColor.r = mix(0.5, 1.0, texColor.r);
    out_Color = texColor;
}
