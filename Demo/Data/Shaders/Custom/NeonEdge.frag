#version 420

in vec2 fragUV;
out vec4 out_Color;

uniform sampler2D BlitTex;
uniform vec4 BlitRect;

uniform vec2 AtlasSize;
uniform vec4 Color = vec4(0.0, 0.5, 1.0, 0.5);
uniform float Intensity = 1.0;

vec4 getPixel(int x, int y, vec2 uvst)
{
    return texture(BlitTex, clamp(((uvst.xy * AtlasSize.xy) + vec2(x, y)) / AtlasSize.xy, BlitRect.xy, BlitRect.zw));
}

void main()
{        
    vec2 blitUV = mix(BlitRect.xy, BlitRect.zw, fragUV);
    vec4 sum = abs(getPixel(0, 1, blitUV) - getPixel(0, -1, blitUV));
    sum += abs(getPixel(1, 0, blitUV) - getPixel(-1, 0, blitUV));
    sum /= 2.0;
    vec4 color = texture(BlitTex, blitUV);
    color.rgb += Color.rgb * Color.a * length(sum) * Intensity;
    out_Color = color;
}