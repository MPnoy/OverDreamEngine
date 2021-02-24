#version 420

layout(binding = 0) uniform sampler2D textureSampler;

uniform mat4 _ColorMatrix;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{
    vec4 color = texture(textureSampler, fragUV);
    color.r /= color.a; color.g /= color.a; color.b /= color.a;
    vec4 o = _ColorMatrix * color;
    out_Color = o;
}
