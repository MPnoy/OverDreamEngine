#version 420

uniform sampler2D BlitTex;
uniform vec4 BlitRect;

uniform mat4 ColorMatrix;
uniform vec4 ColorOffset;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 color = texture(BlitTex, mix(BlitRect.xy, BlitRect.zw, fragUV));
    color = ColorMatrix * color + ColorOffset;
    out_Color = color;
}