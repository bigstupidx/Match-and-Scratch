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
	public bool isAnalizing;
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
		if (currentState == GroupState.Remove)
			Debug.Log(string.Format("<color=yellow> Orden de destruir el PinsGroup [{0}]</color>", index));		
        SetState(GroupState.Remove);
		foreach (Circumference c in members) {
			c.colisionador.enabled = false;
		}
		GameManager.instance.StartCoroutine(DestroyMembers());
    }

    public void SetState(GroupState newState) {
		if (currentState != newState) {
			currentState = newState;
		}
		else {
			Debug.Log(string.Format("<color=yellow> PinsGroup[{0}] : Se ha vuelto a establecer el mismo estado ( {1} ) que ya tenía que sigue activo </color>", index, newState.ToString()));
		}
		isActive = currentState == GroupState.Active;
        
    }

	public IEnumerator DestroyMembers(bool sumPoints = true) {
		if (isActive)
			Debug.Log(string.Format("<color=red> Destruyendo grupo {0} que sigue activo </color>", index));
		
        for( int i = 0; i < members.Count; i++) {
			//childPins.Remove(circumferences[i]);
			if (members[i] != null)
				members[i].gameObject.GetComponent<Pin>().Autodestroy();
			yield return new WaitForSeconds(Pin.TIME_TO_DESTROY/members.Count);
		}
		if (sumPoints) GameManager.instance.Score += members.Count - 2; 
    }
}