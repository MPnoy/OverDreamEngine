#version \\\version

uniform sampler2D images[16];
uniform mat4 matrices[16];
uniform int imageCount;

in vec2 fragUV;

void main()
{
    vec4 o = vec4(0.0);
	
	\\\for_i_0_16
	if (imageCount >= \\\i)
	{
        vec4 uvzw = matrices[\\\i] * vec4((fragUV - vec2(0.5)), 0.0, 1.0);
        vec2 uv = uvzw.xy / uvzw.a + vec2(0.5);
        if (uv.x > 0.0 && uv.x < 1.0 && uv.y > 0.0 && uv.y < 1.0)
        {
            vec4 color = texture2D(images[\\\i], uv);
            o = vec4(color.rgb * color.a, color.a) + o * (1.0 - color.a);
        }		
	}
	\\\next_i

    gl_FragColor = vec4(o.rgb / o.a, o.a);
}