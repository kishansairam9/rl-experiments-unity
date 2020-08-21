using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
[System.Serializable]
public class PID {
	public float pFactor, iFactor, dFactor, minError;
		
	float integral;
	float lastError;
	
	
	public PID(float pFactor, float iFactor, float dFactor) {
		this.pFactor = pFactor;
		this.iFactor = iFactor;
		this.dFactor = dFactor;
	}

	public void Reset(){
		this.integral = 0;
		this.lastError = 0;
	}
	
	
	public float Update(float setpoint, float actual, float timeFrame) {
		Debug.Log("--> " + setpoint + " --- " + actual + " --- ");
		float present = setpoint - actual;
		if (Math.Abs(present) < minError)
			return 0f;
		this.integral += present * timeFrame;
		float deriv = (present - this.lastError) / timeFrame;
		this.lastError = present;
		return present * pFactor + this.integral * iFactor + deriv * dFactor;
	}
}
