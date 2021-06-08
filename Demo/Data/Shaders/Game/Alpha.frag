#version 420

uniform sampler2D blitInput;

uniform float alpha = 1.0;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 ret = texture(blitInput, fragUV);
    ret.a *= alpha;
    out_Color = ret;
}
