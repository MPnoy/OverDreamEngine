#version 420

layout(binding = 0) uniform sampler2D textureSampler;

uniform vec4 color;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{
    vec4 ret = texture(textureSampler, fragUV) * color;
    ret.rgb *= ret.a;
    out_Color = ret;
    
}
