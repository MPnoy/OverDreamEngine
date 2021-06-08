#version 420

in vec3 vertex_position;
in vec2 uv;
uniform mat4 matrix;

out vec2 fragUV;

void main(){
    gl_Position = matrix * vec4(vertex_position, 1.0);
    fragUV = uv;
}
