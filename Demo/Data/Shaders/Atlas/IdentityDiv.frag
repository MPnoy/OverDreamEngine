#version 420

uniform sampler2D BlitTex;
uniform vec4 BlitRect;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 color = texture(BlitTex, mix(BlitRect.xy, BlitRect.zw, fragUV));
    color.rgb /= color.a;
    out_Color = color;
}
