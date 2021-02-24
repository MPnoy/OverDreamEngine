#version 420

layout(binding = 0) uniform sampler2D textureSampler;

uniform int _TexCount;
uniform int _TexIndex;

in vec2 fragUV;
out vec4 out_Color;

void main() 
{      
    vec2 newUV = vec2(fragUV.x * _TexCount - _TexIndex, fragUV.y);
    if (newUV.x < 0 || newUV.y < 0 || newUV.x > 1 || newUV.y > 1)
    {
        out_Color = vec4(0, 0, 0, 0);
        return;
    }
    vec4 o = texture(textureSampler, newUV);
    o.r *= o.a; o.g *= o.a; o.b *= o.a;
    out_Color = o;
}
