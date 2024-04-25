@vertex
@include(includes/camera.inc)
uniform mat4x4 World;

in vec3 vertex_position;
in vec2 vertex_texture1;
in float vertex_color;

out vec2 tex;

void main()
{
    vec4 pos = (ViewProjection * World) * vec4(vertex_position, 1.0);
    gl_Position = pos;
    tex = vertex_texture1;
}

@fragment
uniform sampler2D Texture;
uniform float Time;

in vec2 tex;
out vec4 out_color;

void main()
{
    out_color = texture(Texture, tex);
    
    vec2 direction = vec2(0.5, 1.0);		
	vec2 uv = tex / 20.0;
	float t = Time / 8000.0;
	vec4 perlin = vec4(0.0);
	perlin += texture(Texture, 4.0 * (uv + 8.0 * t * direction)) / 2.0;
    perlin += texture(Texture, 8.0 * (uv + 4.0 * t * direction)) / 4.0;
    perlin += texture(Texture, 16.0 * (uv + 2.0 * t * direction)) / 8.0;
    perlin += texture(Texture, 32.0 * (uv + 1.0 * t * direction)) / 16.0;
    out_color = vec4(1.0) - (vec4(1.0) - perlin) * (vec4(1.0) - perlin);
}
