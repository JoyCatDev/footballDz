using UnityEngine;
using System.Collections;

public class GeTween {

	// t: current time, b: beginning value, c: change in value, d: duration
	// t and d can be frames or seconds/milliseconds
	// change in value = end value - beginning value (e.g. distance to be travelled)
	
	// Back
	static public float BackEaseIn(float t, float b, float c, float d)
	{
		return (BackEaseIn(t, b, c, d, 1.70158f));
	}
	static public float BackEaseIn(float t, float b, float c, float d, float s)
	{
		//if (float.IsNaN(s)) s = 1.70158f;
		return c*(t/=d)*t*((s+1)*t - s) + b;
	}
	
	static public float BackEaseOut(float t, float b, float c, float d)
	{
		return (BackEaseOut(t, b, c, d, 1.70158f));
	}
	static public float BackEaseOut(float t, float b, float c, float d, float s)
	{
		//if (float.IsNaN(s)) s = 1.70158f;
		return c*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
	}
	
	static public float BackEaseInOut(float t, float b, float c, float d)
	{
		return (BackEaseInOut(t, b, c, d, 1.70158f));
	}
	static public float BackEaseInOut(float t, float b, float c, float d, float s)
	{
		//if (float.IsNaN(s)) s = 1.70158f;
		if ((t/=d/2) < 1) return c/2*(t*t*(((s*=(1.525f))+1)*t - s)) + b;
		return c/2*((t-=2)*t*(((s*=(1.525f))+1)*t + s) + 2) + b;
	}
	
	
	// Bounce
	static public float BounceEaseOut(float t, float b, float c, float d)
	{
		if ((t/=d) < (1/2.75f)) {
			return c*(7.5625f*t*t) + b;
		} else if (t < (2/2.75f)) {
			return c*(7.5625f*(t-=(1.5f/2.75f))*t + 0.75f) + b;
		} else if (t < (2.5f/2.75f)) {
			return c*(7.5625f*(t-=(2.25f/2.75f))*t + 0.9375f) + b;
		} else {
			return c*(7.5625f*(t-=(2.625f/2.75f))*t + 0.984375f) + b;
		}
	}
	
	static public float BounceEaseIn(float t, float b, float c, float d)
	{
		return c - BounceEaseOut(d-t, 0, c, d) + b;
	}
	
	static public float BounceEaseInOut(float t, float b, float c, float d)
	{
		if (t < d/2) return BounceEaseIn(t*2, 0, c, d) * 0.5f + b;
		else return BounceEaseOut(t*2-d, 0, c, d) * 0.5f + c*0.5f + b;
	}
	
	
	// Circ
	static public float CircEaseIn(float t, float b, float c, float d)
	{
		return -c * (Mathf.Sqrt(1 - (t/=d)*t) - 1) + b;
	}
	
	static public float CircEaseOut(float t, float b, float c, float d)
	{
		return c * Mathf.Sqrt(1 - (t=t/d-1)*t) + b;
	}
	
	static public float CircEaseInOut(float t, float b, float c, float d)
	{
		if ((t/=d/2) < 1) return -c/2 * (Mathf.Sqrt(1 - t*t) - 1) + b;
		return c/2 * (Mathf.Sqrt(1 - (t-=2)*t) + 1) + b;
	}
	
	
	// Cubic
	static public float CubicEaseIn(float t, float b, float c, float d)
	{
		return c*(t/=d)*t*t + b;
	}
	
	static public float CubicEaseOut(float t, float b, float c, float d)
	{
		return c*((t=t/d-1)*t*t + 1) + b;
	}
	
	static public float CubicEaseInOut(float t, float b, float c, float d)
	{
		if ((t/=d/2) < 1) return c/2*t*t*t + b;
		return c/2*((t-=2)*t*t + 2) + b;
	}
	
	
	// Elastic
	static public float ElasticEaseIn(float t, float b, float c, float d)
	{
		return (ElasticEaseIn(t, b, c, d, Mathf.Abs(c) - 1, d*0.3f));
	}
	static public float ElasticEaseIn(float t, float b, float c, float d, float a)
	{
		return (ElasticEaseIn(t, b, c, d, a, d*0.3f));
	}
	static public float ElasticEaseIn(float t, float b, float c, float d, float a, float p)
	{
		if (t==0) return b;  if ((t/=d)==1) return b+c;  //if (!p) p=d*0.3f;
		float s;
		//if (!a || a < Mathf.Abs(c)) { a=c; s=p/4; }
		if (a < Mathf.Abs(c)) { a=c; s=p/4; }
		else s = p/(2*Mathf.PI) * Mathf.Asin(c/a);
		return -(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )) + b;
	}
	
	static public float ElasticEaseOut(float t, float b, float c, float d)
	{
		return (ElasticEaseOut(t, b, c, d, Mathf.Abs(c) - 1, d*0.3f));
	}
	static public float ElasticEaseOut(float t, float b, float c, float d, float a)
	{
		return (ElasticEaseOut(t, b, c, d, a, d*0.3f));
	}
	static public float ElasticEaseOut(float t, float b, float c, float d, float a, float p)
	{
		if (t==0) return b;  if ((t/=d)==1) return b+c;  //if (!p) p=d*0.3f;
		float s;
		//if (!a || a < Mathf.Abs(c)) { a=c; s=p/4; }
		if (a < Mathf.Abs(c)) { a=c; s=p/4; }
		else s = p/(2*Mathf.PI) * Mathf.Asin(c/a);
		return (a*Mathf.Pow(2,-10*t) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p ) + c + b);
	}
	
	static public float ElasticEaseInOut(float t, float b, float c, float d)
	{
		return (ElasticEaseInOut(t, b, c, d, Mathf.Abs(c) - 1, d*(0.3f*1.5f)));
	}
	static public float ElasticEaseInOut(float t, float b, float c, float d, float a)
	{
		return (ElasticEaseInOut(t, b, c, d, a, d*(0.3f*1.5f)));
	}
	static public float ElasticEaseInOut(float t, float b, float c, float d, float a, float p)
	{
		if (t==0) return b;  if ((t/=d/2)==2) return b+c;  //if (!p) p=d*(0.3f*1.5f);
		float s;
		//if (!a || a < Mathf.Abs(c)) { a=c; s=p/4; }
		if (a < Mathf.Abs(c)) { a=c; s=p/4; }
		else s = p/(2*Mathf.PI) * Mathf.Asin(c/a);
		if (t < 1) return -0.5f*(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )) + b;
		return a*Mathf.Pow(2,-10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )*0.5f + c + b;
	}
	
	
	// Expo
	static public float ExpoEaseIn(float t, float b, float c, float d)
	{
		return (t==0) ? b : c * Mathf.Pow(2, 10 * (t/d - 1)) + b;
	}
	
	static public float ExpoEaseOut(float t, float b, float c, float d)
	{
		return (t==d) ? b+c : c * (-Mathf.Pow(2, -10 * t/d) + 1) + b;
	}
	
	static public float ExpoEaseInOut(float t, float b, float c, float d)
	{
		if (t==0) return b;
		if (t==d) return b+c;
		if ((t/=d/2) < 1) return c/2 * Mathf.Pow(2, 10 * (t - 1)) + b;
		return c/2 * (-Mathf.Pow(2, -10 * --t) + 2) + b;
	}
	
	
	// Linear
	static public float LinearEaseNone(float t, float b, float c, float d)
	{
		return c*t/d + b;
	}
	
	static public float LinearEaseIn(float t, float b, float c, float d)
	{
		return c*t/d + b;
	}
	
	static public float LinearEaseOut(float t, float b, float c, float d)
	{
		return c*t/d + b;
	}
	
	static public float LinearEaseInOut(float t, float b, float c, float d)
	{
		return c*t/d + b;
	}
	
	
	// Quad
	static public float QuadEaseIn(float t, float b, float c, float d)
	{
		return c*(t/=d)*t + b;
	}
	
	static public float QuadEaseOut(float t, float b, float c, float d)
	{
		return -c *(t/=d)*(t-2) + b;
	}
	
	static public float QuadEaseInOut(float t, float b, float c, float d)
	{
		if ((t/=d/2) < 1) return c/2*t*t + b;
		return -c/2 * ((--t)*(t-2) - 1) + b;
	}

	
	// Quart
	static public float QuartEaseIn(float t, float b, float c, float d)
	{
		return c*(t/=d)*t*t*t + b;
	}
	
	static public float QuartEaseOut(float t, float b, float c, float d)
	{
		return -c * ((t=t/d-1)*t*t*t - 1) + b;
	}
	
	static public float QuartEaseInOut(float t, float b, float c, float d)
	{
		if ((t/=d/2) < 1) return c/2*t*t*t*t + b;
		return -c/2 * ((t-=2)*t*t*t - 2) + b;
	}
	
	
	// Quint
	static public float QuintEaseIn(float t, float b, float c, float d)
	{
		return c*(t/=d)*t*t*t*t + b;
	}
	static public float QuintEaseOut(float t, float b, float c, float d)
	{
		return c*((t=t/d-1)*t*t*t*t + 1) + b;
	}
	static public float QuintEaseInOut(float t, float b, float c, float d)
	{
		if ((t/=d/2) < 1) return c/2*t*t*t*t*t + b;
		return c/2*((t-=2)*t*t*t*t + 2) + b;
	}
	
	
	// Sine
	static public float SineEaseIn(float t, float b, float c, float d)
	{
		return -c * Mathf.Cos(t/d * (Mathf.PI/2)) + c + b;
	}
	
	static public float SineEaseOut(float t, float b, float c, float d)
	{
		return c * Mathf.Sin(t/d * (Mathf.PI/2)) + b;
	}
	
	static public float SineEaseInOut(float t, float b, float c, float d)
	{
		return -c/2 * (Mathf.Cos(Mathf.PI*t/d) - 1) + b;
	}

}
