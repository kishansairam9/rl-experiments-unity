using UnityEngine;

public class SimpleTrigger : MonoBehaviour
{
    Vector3 pos, size;
    bool active = false;
    string colliderName;
    BoxCollider boxCollider;

    public void Init(Vector3 pos, Vector3 size, Transform parent, string colliderName, string name="Trigger")
    {
        this.pos = pos;
        this.size = size;
        this.gameObject.transform.parent = parent;
        this.gameObject.AddComponent<BoxCollider>();
        this.gameObject.AddComponent<MeshRenderer>();
        this.boxCollider = this.gameObject.GetComponent<BoxCollider>();
        this.boxCollider.size = this.size;
        this.boxCollider.center = this.pos;
        this.boxCollider.isTrigger = true;
        this.colliderName = colliderName;
        this.gameObject.layer = LayerMask.NameToLayer("Triggers");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == this.colliderName)
        {
            this.active = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name == this.colliderName)
        {
            this.active = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == this.colliderName)
        {
            this.active = false;
        }
    }

    public bool isActive()
    {
        return this.active;
    }
}
