#version 420

uniform sampler2D images[16];
uniform mat4 matrices[16];
uniform int imageCount;

in vec2 fragUV;
out vec4 out_Color;

void main()
{
    vec4 o = vec4(0.0);
    
    for (int i = 0; i < imageCount; i++)
    {
        vec4 uvzw = matrices[i] * vec4((fragUV - vec2(0.5)), 0.0, 1.0);
        vec2 uv = uvzw.xy / uvzw.a + vec2(0.5);
        if (uv.x > 0.0 && uv.x < 1.0 && uv.y > 0.0 && uv.y < 1.0)
        {
            vec4 color = texture(images[i], uv);
            o = vec4(color.rgb * color.a, color.a) + o * (1.0 - color.a);
        }
    }

    out_Color = vec4(o.rgb / o.a, o.a);
}