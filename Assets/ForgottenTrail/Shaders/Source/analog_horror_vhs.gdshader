shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;
uniform float effect_strength : hint_range(0.0, 1.0) = 0.58;
uniform float grain_strength : hint_range(0.0, 0.15) = 0.035;
uniform float scanline_strength : hint_range(0.0, 0.2) = 0.055;
uniform float chromatic_shift : hint_range(0.0, 3.0) = 0.85;
uniform float color_muddle : hint_range(0.0, 1.0) = 0.16;


float random_noise(vec2 coordinate) {
	return fract(sin(dot(coordinate, vec2(12.9898, 78.233))) * 43758.5453);
}


void fragment() {
	vec2 pixel_size = 1.0 / vec2(textureSize(screen_texture, 0));
	float tape_wobble = sin(SCREEN_UV.y * 22.0 + TIME * 1.7);
	tape_wobble += sin(SCREEN_UV.y * 71.0 - TIME * 0.8) * 0.35;
	vec2 uv = SCREEN_UV;
	uv.x += tape_wobble * pixel_size.x * 0.55 * effect_strength;
	uv = clamp(uv, pixel_size, vec2(1.0) - pixel_size);

	float split = chromatic_shift * pixel_size.x * effect_strength;
	vec3 sampled_color;
	sampled_color.r = texture(screen_texture, uv + vec2(split, 0.0)).r;
	sampled_color.g = texture(screen_texture, uv).g;
	sampled_color.b = texture(screen_texture, uv - vec2(split, 0.0)).b;

	float luminance = dot(sampled_color, vec3(0.299, 0.587, 0.114));
	vec3 muddy_color = vec3(
		luminance * 1.05,
		luminance * 0.91,
		luminance * 0.72
	);
	sampled_color = mix(sampled_color, muddy_color, color_muddle * effect_strength);

	float scanline = sin(FRAGCOORD.y * 3.14159265);
	sampled_color *= 1.0 - scanline_strength * effect_strength * (0.5 + scanline * 0.5);

	float grain = random_noise(
		floor(FRAGCOORD.xy) + floor(TIME * 24.0) * vec2(17.0, 29.0)
	);
	sampled_color += (grain - 0.5) * grain_strength * effect_strength;

	vec2 centered = SCREEN_UV * 2.0 - 1.0;
	float vignette = smoothstep(1.25, 0.35, dot(centered, centered));
	sampled_color *= mix(0.82, 1.0, vignette);

	COLOR = vec4(sampled_color, 1.0);
}
