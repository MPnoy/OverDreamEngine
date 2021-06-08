#version 420

uniform sampler2D blitInput;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 o = texture(blitInput, fragUV);
    o.rgb /= o.a;
    out_Color = o;
}
