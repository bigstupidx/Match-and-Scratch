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
	public bool isActive {
		get { return currentState == GroupState.Active;}
	}
	public bool isAnalizing;

    public void CombineWith(int value) {
        SetState(GroupState.Combined);
    }
    
    public GroupState currentState = GroupState.Off;

    public PinsGroups(int id, Circumference member = null) {
        SetState(GroupState.Active);
        index = id;
		//combinedID = -1;
        if (member != null)
            AddMember(member);
    }

    public void AddMember(Circumference c) {
        if (c.tag == "Rotator")
            Debug.Log("<color=red>Error WTF 102: Rotator metido en un grupo O_o </color>");
		
        if (!members.Contains(c)) {
            members.Add(c);
			c.colorGroup = index;
			c.name += "_group" + index;
        }
    }

    public void AddMembers(List<Circumference> membersList) {
        foreach (Circumference member in membersList)
        {
			member.name += "_group" + index;
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
    }

	public IEnumerator DestroyMembers(bool sumPoints = true) {
		if (isActive)
			Debug.Log(string.Format("<color=red> Destruyendo grupo {0} que sigue activo </color>", index));

		if (sumPoints) {
			GameManager.instance.AddScore(members.Count - 2);
		}

		for( int i = members.Count-1; i >=0; i--) {
			if (members[i] != null)
				members[i].gameObject.GetComponent<Pin>().Autodestroy();
			yield return new WaitForSeconds(Pin.TIME_TO_DESTROY/members.Count);
		}

    }
}