using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrantObject : MonoBehaviour
{

    [SerializeField]
    private new Text name;
    [SerializeField]
    private Text reward;
    [SerializeField]
    private Image checkmark;
    private bool accepted = false;
    [SerializeField]
    private Text moneytext;
    private int moneyreward;
    [SerializeField]
    private Text requirementsText;
    private GrantsManagment.Status status;
    [SerializeField]
    private Button acceptbutton;
    [SerializeField]
    private GameObject acp, comp, unlod, lod;

    public void SetContent(GrantsManagment.Grant grant)
    {
        int moneyreward = grant.reward;
        reward.text = moneyreward.ToString();
        name.text = grant.name;
        checkmark.enabled = false;
        requirementsText.text = "";
        for (int i = 0; i < grant.requirements.Count; i++)
        {
            Debug.Log(grant.requirements[i].displaytext);
            requirementsText.text += ("*  " + grant.requirements[i].displaytext + System.Environment.NewLine);
        }
        if (grant.dependency.Count == 0)
        {
            SetStatus(GrantsManagment.Status.Unlocked);
        }
        else
        {
            status = GrantsManagment.Status.Locked;
            acceptbutton.gameObject.SetActive(false);
            SetStatus(GrantsManagment.Status.Locked);
        }
    }

    public void Accept()
    {
        Debug.Log("Start Accept: " + accepted);
        if (accepted == false)
        {
            status = GrantsManagment.Status.Accepted;
            accepted = true;
            int money = int.Parse(moneytext.text);
            money += moneyreward / 2;
            moneytext.text = money.ToString();
            checkmark.enabled = true;
            SetStatus(GrantsManagment.Status.Accepted);
        }
        else
        {

        }
    }

    public void SetStatus(GrantsManagment.Status newstatus)
    {
        status = newstatus;
        switch (newstatus)
        {
            case GrantsManagment.Status.Locked:
                this.transform.SetParent(lod.transform, false);
                break;
            case GrantsManagment.Status.Unlocked:
                this.transform.SetParent(unlod.transform, false);
                break;
            case GrantsManagment.Status.Accepted:
                this.transform.SetParent(acp.transform, false);
                break;
            case GrantsManagment.Status.Completed:
                this.transform.SetParent(comp.transform, false);
                break;
        }
    }
}
