#include "noise/psrdnoise2D.hlsl"
float HeightIntensity< UiGroup("Cut,10/Noise,10/3"); UiType(Slider); Default(1); Range(1, 1000); >;
float NoiseSize< UiGroup("Cut,10/Noise,10/4"); UiType(Slider); Default(1); Range(0.01f, 100); >;
float NoiseSize2< UiGroup("Cut,10/Noise,10/5"); UiType(Slider); Default(1); Range(0.01f, 100); >;
float NoiseMod< UiGroup("Cut,10/Noise,10/6"); UiType(Slider); Default(1); Range(0.01f, 1000); >;
int NoiseOctaves< UiGroup("Cut,10/Noise,10/7"); UiType(Slider); Default(1); Range(1, 16); >;


float NoiseSpeed< UiGroup("Cut,10/NoiseSpeed,10/1"); UiType(Slider); Default(1); Range(0.01f, 10000); >;
float NoiseSpeed2< UiGroup("Cut,10/NoiseSpeed,10/2"); UiType(Slider); Default(1); Range(0.01f, 10000); >;

float Noiseperx< UiGroup("Cut,10/NoiseSpeed,10/3"); UiType(Slider); Default(1); Range(0.01f, 1000); >;
float Noisepery< UiGroup("Cut,10/NoiseSpeed,10/4"); UiType(Slider); Default(1); Range(0.01f, 1000); >;

#define ITERATIONS_NORMAL 30
float rand(float2 co)
{
	return (psnoise(co,float2(Noiseperx,Noisepery))+1)/2;
}

float noiseWithOctave(float2 pos,int Octaves = 1, float Persistance = 0.5f, float Scale = 1.0f,float frequency = 1.0f,float amplitude = 1.0f)
{
    float total = 0.0f;
    float maxValue = 0.0f;
    for (int i = 0; i < Octaves; i++)
    {
        total += rand(pos * frequency) * amplitude;
        maxValue += amplitude;
        amplitude *= Persistance;
        frequency *= 2.0f;
    }
    return total / maxValue;
}


float GetNoiseSpot(float2 vPos){
    vPos = vPos/NoiseSize/100;
    float fracc = frac(g_flTime/NoiseSpeed)*(Noiseperx*(Noisepery*Noisepery))/0.5;
	float noise = noiseWithOctave(vPos.xy+fracc,NoiseOctaves)* HeightIntensity;

    float fracc2 = frac(g_flTime/(NoiseSpeed2))*(Noiseperx*(Noisepery*Noisepery))/0.5;
    float TimeOffsetNoise = noiseWithOctave((vPos.xy/NoiseSize2)+fracc2,NoiseOctaves)* NoiseMod;

    return noise+TimeOffsetNoise /2;

   // float fracc2 = frac(g_flTime/(NoiseSpeed*100))*(Noiseperx*(Noisepery*Noisepery))/0.5;
   // noise += noiseWithOctave(vPos.xy*10+float2(124,1002)+(fracc2))* HeightIntensity;
    return noise;
}
float3 WaterNormal(float2 pos, float epsilon)
{
    float H = 0;
    float2 ex = float2(epsilon, 0);
    H = GetNoiseSpot(pos.xy) ;
    float3 a = float3(pos.x, H, pos.y);
    float3 n = normalize(cross(normalize(a-float3(pos.x - epsilon, GetNoiseSpot(pos.xy - ex.xy), pos.y)), 
                           normalize(a-float3(pos.x, GetNoiseSpot(pos.xy + ex.yx), pos.y + epsilon))));
    return float3(n.x,n.z,n.y);
}