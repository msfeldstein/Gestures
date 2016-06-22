using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ObjectPalette : MonoBehaviour
{
    public GameObject[] prototypes;
    public Material objectMaterial;
    [Range(0, 1)]
    public float swipeSpeed = 0.3f;

    private SteamVR_TrackedController controller;
    private GameObject[] items;
    private GameObject container;
    private GameObject currentItem;
    private IEnumerator hidingCoroutine;
    private IEnumerator scalingCoroutine;
    private GameObject hoveredItem;
    private GameObject grabbedItem;

    private float lastX;
    private float lastY;

    // Use this for initialization
    void Start()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.PadTouched += OnPadTouched;
        controller.PadUntouched += OnPadUntouched;
        controller.TriggerClicked += OnTriggerClicked;
        controller.TriggerUnclicked += OnTriggerUnclicked;
        controller.Gripped += OnGripped;
        controller.Ungripped += OnUngripped;

        container = new GameObject();
        container.transform.parent = transform;
        items = new GameObject[prototypes.Length];
        for (int i = 0; i < prototypes.Length; i++)
        {
            GameObject item = (GameObject)Instantiate(prototypes[i]);
            items[i] = item;
            item.transform.parent = container.transform;
            item.transform.position = new Vector3(0.2f * i, 0, 0.1f);
            Material m = item.GetComponent<MeshRenderer>().material;
            Color c = m.color;
            c.a = 0;
            m.color = c;
        }
    }

    void OnPadTouched(object o, ClickedEventArgs e)
    {
        Debug.Log("Touched");
        lastX = e.padX;
        lastY = e.padY;
        if (hidingCoroutine != null) StopCoroutine(hidingCoroutine);
        hidingCoroutine = null;
        for (int i = 0; i < items.Length; i++)
        {
            items[i].GetComponent<MeshRenderer>().material.DOFade(1f, .3f).SetDelay(i * .04f);
        }
    }

    void OnPadUntouched(object o, ClickedEventArgs e)
    {
        Debug.Log("Untouched");
        hidingCoroutine = MaybeHidePalette();
        StartCoroutine(hidingCoroutine);
        Debug.Log("----");
    }

    IEnumerator MaybeHidePalette()
    {
        yield return new WaitForSeconds(1);
        if (!controller.padTouched)
        {
            HidePalette();
        }
    }

    void HidePalette()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i].GetComponent<MeshRenderer>().material.DOFade(0f, .3f).SetDelay((items.Length - i - 1) * .04f);
        }
    }

    void OnTriggerClicked(object o, ClickedEventArgs e)
    {
        HidePalette();
        int index = (int)Mathf.Clamp(Mathf.Round(-container.transform.localPosition.x * 5f), 0, items.Length - 1);
        currentItem = Instantiate(items[index]);
        currentItem.transform.parent = transform;
        currentItem.GetComponent<MeshRenderer>().material = objectMaterial;
        currentItem.transform.position = items[index].transform.position;
        currentItem.transform.rotation = items[index].transform.rotation;
        scalingCoroutine = ScaleUp();
        StartCoroutine(scalingCoroutine);
    }

    void OnTriggerUnclicked(object o, ClickedEventArgs e)
    {
        currentItem.transform.parent = null;
        StopCoroutine(scalingCoroutine);
    }

    IEnumerator ScaleUp()
    {
        while (true)
        {
            currentItem.transform.localScale += Vector3.one * .001f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    void Update()
    {
        if (controller.padTouched)
        {
            float dx = controller.controllerState.rAxis0.x - lastX;
            dx *= swipeSpeed;
            float containerX = container.transform.localPosition.x;
            if (containerX > 0 && dx > 0)
            {
                dx *= 1f / (10f * (containerX + 1f));
            }

            if (containerX < -.2f * items.Length && dx < 0)
            {
                dx *= 1f / (10f * ((items.Length - 1) * .2f - containerX + 1f));
            }
            container.transform.Translate(new Vector3(dx, 0, 0));
            lastX = controller.controllerState.rAxis0.x;
            lastY = controller.controllerState.rAxis0.y;
            Debug.Log(container.transform.localPosition.x);
        }
    }


    void OnTriggerEnter(Collider c)
    {
        hoveredItem = c.gameObject;
        Material m = hoveredItem.GetComponent<MeshRenderer>().sharedMaterial;
        m.EnableKeyword("_EMISSION");
        Color col = m.color;
        col *= .8f;
        m.SetColor("_EmissionColor", col);
        Debug.Log("Trigger");
    }

    void OnTriggerExit(Collider c)
    {
        if (!hoveredItem) return;
        if (controller.gripped) return;
        Material m = hoveredItem.GetComponent<MeshRenderer>().sharedMaterial;
        m = new Material(m);
        Color col = m.color;
        col *= 0f;
        m.SetColor("_EmissionColor", col);
        Debug.Log("Leave");
        hoveredItem.GetComponent<MeshRenderer>().sharedMaterial = m;
        hoveredItem = null;

    }

    void OnGripped(object o, ClickedEventArgs e)
    {
        if (hoveredItem != null)
        {
            grabbedItem = hoveredItem;
            hoveredItem.transform.parent = transform;
        }
    }

    void OnUngripped(object o, ClickedEventArgs e)
    {
        if (grabbedItem != null)
        {
            grabbedItem.transform.parent = null;
        }
    }
}
