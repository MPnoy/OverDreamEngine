#version 420

uniform sampler2D BlitTex;
uniform vec4 BlitRect;

uniform float alpha;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 color = texture(BlitTex, mix(BlitRect.xy, BlitRect.zw, fragUV));
    color.a *= alpha;
    out_Color = color;
}