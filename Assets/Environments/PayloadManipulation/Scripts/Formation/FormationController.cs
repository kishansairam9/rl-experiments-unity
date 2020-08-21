using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Pose
{
    private float x, y, theta;
    Pose(float x, float y, float theta)
    {
        this.x = x;
        this.y = y;
        this.theta = theta;
    }
}

public class FormationController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject leaderObject;
    public float L_ijd;
    public float phi_ijd;
    public Vector3 k = new Vector3(10f, 5f, 1f);
    public Vector3 gains = new Vector3(50f, 5f, 5f);

    private float alpha_j = 0f, beta_j = 0f;
    private float X_je = 0f, Y_je = 0f, theta_je = 0f;
    private float leaderLinearVelocity, leaderAngularVelocity;
    private Rigidbody l_rb, m_rb;
    private Transform leader;

    void Start()
    {
        m_rb = GetComponent<Rigidbody>();
        l_rb = leaderObject.GetComponent<Rigidbody>();
        leader = leaderObject.transform;
        print("----");
    }

    void Update()
    {
        UpdateLeader();
    }

    void FixedUpdate()
    {
        var Lth = leader.eulerAngles.y * Mathf.Deg2Rad;
        var Lijx = leader.position.x - transform.position.x - gains.z * Mathf.Cos(Lth);
        var Lijy = leader.position.z - transform.position.z - gains.z * Mathf.Sin(Lth);
		var Lij = Mathf.Sqrt(Lijx * Lijx + Lijy * Lijy);
        var phi_ij = Mathf.Atan2(Lijy, Lijx) - Lth + Mathf.PI;
        var theta_ij = Lth - transform.eulerAngles.y * Mathf.Deg2Rad;
        var f1 = Mathf.Max(+k.x * X_je, 0f);
		var f2 = Mathf.Max(+k.y * Y_je, 0f);
		var g1 = Mathf.Max(-k.x * X_je, 0f);
		var g2 = Mathf.Max(-k.y * Y_je, 0f);
        // if (leaderAngularVelocity == 0f && leaderLinearVelocity == 0f)
        //     return;
        var dt = Time.deltaTime;
        var term_alpha = -gains.x * alpha_j + (gains.y - alpha_j) * f1 - (gains.z + alpha_j) * g1;
        var term_beta = -gains.x * beta_j + (gains.y - beta_j) * f2 - (gains.z + beta_j) * g2;

        alpha_j += term_alpha * dt;
        beta_j += term_beta * dt;

        var vel = k.x * alpha_j + leaderLinearVelocity * Mathf.Cos(theta_ij) - L_ijd * leaderAngularVelocity * Mathf.Sin(phi_ijd + theta_ij);
        var omega = (leaderLinearVelocity * Mathf.Sin(theta_ij) + L_ijd * leaderAngularVelocity * Mathf.Cos(phi_ijd + theta_ij) + k.y * beta_j + k.z * theta_je ) / gains.z;


        var velvel = transform.forward * Mathf.Clamp(vel, -1f, 1f);
        m_rb.velocity = velvel;
        var my_omg = m_rb.angularVelocity;
        my_omg.y = Mathf.Clamp(omega, -1f, 1f);
        m_rb.angularVelocity = my_omg;
        print("vel: ("+ velvel.x + ", " + velvel.y +", " + velvel.z+") " + " | omega: "+ my_omg.y);


        X_je = L_ijd * Mathf.Cos(phi_ijd + theta_ij) - Lij * Mathf.Cos(phi_ij + theta_ij);
        Y_je = L_ijd * Mathf.Sin(phi_ijd + theta_ij) - Lij * Mathf.Sin(phi_ij + theta_ij);
        theta_je = Lth - transform.eulerAngles.y * Mathf.Deg2Rad;
        if (theta_je < -Mathf.PI)
            theta_je = theta_je + 2 * Mathf.PI;
        if (theta_je > Mathf.PI)
            theta_je  = theta_je - 2 * Mathf.PI;
    }

    void UpdateLeader()
    {
        leaderLinearVelocity = leader.InverseTransformVector(l_rb.velocity).z + 0.001f*Random.Range(-1f, 1f);
        leaderAngularVelocity = l_rb.angularVelocity.y + 0.001f*Random.Range(-1f, 1f);
        print("Leader Vel: " + leaderLinearVelocity + " Omega: " + leaderAngularVelocity);
    }

    void CalculateError(Transform desiredTransform)
    {

    }
}
