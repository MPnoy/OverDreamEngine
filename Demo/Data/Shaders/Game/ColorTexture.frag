#version 420

uniform sampler2D blitInput;

uniform mat4 _ColorMatrix;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 color = texture(blitInput, fragUV);
    color.r /= color.a; color.g /= color.a; color.b /= color.a;
    vec4 o = _ColorMatrix * color;
    out_Color = o;
}
