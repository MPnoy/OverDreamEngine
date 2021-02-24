#version 420

layout(binding = 0) uniform sampler2D textureSampler;

uniform float alpha = 1.0;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{
    vec4 ret = texture(textureSampler, fragUV);
    ret.a *= alpha;
    out_Color = ret;
}
