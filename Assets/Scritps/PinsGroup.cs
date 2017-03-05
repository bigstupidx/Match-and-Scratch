using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PinsGroups {
    public enum GroupState
    {
        Off,
        Active,
        Combined,
        Remove
    }
    
    public List<Circumference> members = new List<Circumference>();
    public int index;
    public bool isActive;
    int combinedID = -1;

    public void CombineWith(int value) {
        combinedID = value;
        SetState(GroupState.Combined);
    }
    
    public GroupState currentState = GroupState.Off;

    public PinsGroups(int id, Circumference member = null) {
        SetState(GroupState.Active);
        index = id;
        if (member != null)
            AddMember(member);
    }

    public void AddMember(Circumference c) {
        if (c.tag == "Rotator")
            Debug.Log("<color=red>Error WTF 102: Rotator metido en un grupo O_o </color>");
        if (!members.Contains(c)) {
            members.Add(c);
            if (c.colorGroupText != null)
                c.colorGroupText.text = index.ToString();
        }
    }

    public void AddMembers(List<Circumference> membersList) {
        foreach (Circumference member in membersList)
        {
            AddMember(member);
        }
    }

    public bool Contains(Circumference c) {
        return members.Contains(c);
    }

    public int Count {
        get {return members.Count;}
    }

    public void Erase() {
        SetState(GroupState.Remove);
        GameManager.instance.StartCoroutine(DestroyMembers());
    }

    public void SetState(GroupState newState) {
        if (currentState != newState) {
            currentState = newState;
        }
    }

    IEnumerator DestroyMembers() {
        for( int i = 0; i < members.Count; i++) {
			//childPins.Remove(circumferences[i]);
            members[i].gameObject.GetComponent<Pin>().Autodestroy();
			yield return new WaitForSeconds(Pin.TIME_TO_DESTROY/members.Count);
		}
        GameManager.instance.Score += members.Count - 2; 
    }
}