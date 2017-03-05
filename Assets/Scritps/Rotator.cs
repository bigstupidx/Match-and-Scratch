using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : Circumference {

	public float speed = 100f;
	public float minSpawnTime = 0.2f;
	private float spawnTimeDelay;
	private Circumference me;
	private List<Circumference> circumferencesCollided = new List<Circumference>();
	private Dictionary<int, PinsGroups> pinsGroups = new Dictionary<int, PinsGroups>();

	public override void Initialize() {
		pinsGroups.Clear();
		me = GetComponent<Circumference>();
	}

	void FixedUpdate() {
		transform.Rotate(0f, 0f, speed * Time.deltaTime);
	}

	public void AddPin(Circumference newPin, Collider2D col) {
		newPin.cc.isTrigger = false;
		Pin cn =newPin.GetComponent<Pin>();
		cn.isPinned = true;
		newPin.transform.SetParent(transform);

		SearchNearestPins(newPin);
		Reposition(newPin);
		//SearchNearestPins(newPin);
		ProcessPin(newPin);

		if(!GameManager.instance.gameHasEnded) {
			spawnTimeDelay = ProcessPinsGroups();
			GameManager.instance.CheckDifficulty();
			GameManager.instance.spawner.SpawnPin(spawnTimeDelay);
		}
	}

	void SearchNearestPins(Circumference newCircumference) {
		circumferencesCollided.Clear();
		// Comprobamos la distancia con el resto de bolas
		foreach (KeyValuePair<int,PinsGroups> pg in pinsGroups) {
			if (pg.Value.currentState == PinsGroups.GroupState.Active) {
				foreach (Circumference c in pg.Value.members) {
					if ( IsColliding(newCircumference, c, 0.02f) ) {
						if (c.colorType !=  newCircumference.colorType) {
							GameManager.instance.EndGame();
							break;
						}
						circumferencesCollided.Add (c);
					}
				}
			}
		}

		// Y tambien la distanciacon el rotor
		if ( IsColliding(newCircumference, me, 0.02f) )
			circumferencesCollided.Add (me);
		
		// Si hay 3 o mas, nos quedamos sólo con los dos mas cercanos
		if (circumferencesCollided.Count > 2)  {
			circumferencesCollided.Sort( (A,B) => DistanceBetween(newCircumference.GetPosition(), A.GetPosition()).CompareTo(DistanceBetween(newCircumference.GetPosition(), B.GetPosition())) );
			circumferencesCollided = circumferencesCollided.GetRange(0,2);
		}
	}

	void Reposition(Circumference newPin) {
		switch (circumferencesCollided.Count) {
			case 1:
				/*
				//debug posicion pin colisionado
				DrawTheGizmo (new GizmoToDraw( GizmoType.sphere, pinsCollided[0].GetPosition(), pinsCollided[0].GetRadius(), Color.gray ) );
				//debug posicion new pin en el momento de la collision
				DrawTheGizmo( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.yellow ) );
				*/
				// Reposición
				newPin.transform.position = circumferencesCollided[0].GetPosition() + ( (newPin.GetPosition() - circumferencesCollided[0].GetPosition() ).normalized * ( newPin.GetRadius() + circumferencesCollided[0].GetRadius() ) );
				//debug posicion new pin en despues de la colocación
				//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.GetPosition(), newPin.GetRadius(), Color.green ) );
				if ( circumferencesCollided[0].tag == "Rotator" ) newPin.GetComponent<Pin>().DrawSpear();
			break;
			case 2:
				Circumference A = circumferencesCollided[0];
				Circumference B = circumferencesCollided[1];
				//Solución de Fernando Rojas basada en: https://es.wikipedia.org/wiki/Teorema_del_coseno
				float Lc = (B.GetPosition() - A.GetPosition()).magnitude; //A.GetRadius() + B.GetRadius();
				float La = B.GetRadius() + newPin.GetRadius();
				float Lb = newPin.GetRadius() + A.GetRadius();

				float a = Mathf.Rad2Deg * Mathf.Acos(( Lb*Lb + Lc*Lc - La*La ) / (2 * Lb * Lc));

				Vector3 ab = (B.GetPosition()-A.GetPosition ()).normalized;

				Quaternion rot = Quaternion.AngleAxis(a, Vector3.forward);
				Vector3 Solution1 = A.GetPosition() + rot * ab * Lb;

				rot = Quaternion.AngleAxis(a, -Vector3.forward);
				Vector3 Solution2 = A.GetPosition() + rot * ab * Lb;

				#region "Otra solución - Solo funciona con circulos con igual radio"	
				/*/
				//Solución para el ajuste de posición. http://stackoverflow.com/questions/18558487/tangent-circles-for-two-other-circles
				// 1 Calculate distance from A to B -> |AB|:
				float AB = Vector3.Distance(A.GetPosition(), B.GetPosition());
				// 2 Checks whether a solution exist, it exist only if:
				Debug.Assert(AB <= 4 * newPin.GetComponent<Circumference>().GetRadius());
				// 3 If it exist, calculate half-point between points A and B:
				Vector2 H = new Vector2( A.GetPosition().x + ( (B.GetPosition().x - A.GetPosition().x) / 2 ), A.GetPosition().y + ( (B.GetPosition().y - A.GetPosition().y) / 2 ) );
				// 4 Create normalized perpendicular vector to line segment AB:
				Vector2 HC_perp_norm = new Vector2( (B.GetPosition().y - A.GetPosition().y) / AB, -(B.GetPosition().x - A.GetPosition().x) / AB );
				// 5 Calculate distance from this H point to C point -> |HC|:		                           
				float HCpos = Mathf.Abs( 0.5f * Mathf.Sqrt( 16 * ( newPin.GetRadius() * newPin.GetRadius() ) - (AB*AB) ) );
				float HCneg = -( 0.5f * Mathf.Sqrt( 16 * ( newPin.GetRadius() * newPin.GetRadius() ) - (AB*AB) ) );
				// Posibles soluciones
				Vector3 Solution1 = new Vector3 (H.x + (HCpos * HC_perp_norm.x), H.y + (HCpos * HC_perp_norm.y), 0);
				Vector3 Solution2 = new Vector3 (H.x + (HCneg * HC_perp_norm.x), H.y + (HCneg * HC_perp_norm.y), 0);
				*/
				#endregion
				// nos quedamos con la mas cercana al spawner
				Vector3 sol = DistanceBetween(Solution1, GameManager.instance.spawner.transform.position) < 
							  DistanceBetween(Solution2, GameManager.instance.spawner.transform.position) ? Solution1 : Solution2;
				// Posición final
				newPin.transform.position = sol;
				/*
				/// DEBUG ///
				//debug posicion new pin en el momento de la collision
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newCircumference.GetPosition(), newCircumference.GetRadius(), Color.white ) );
				//debug posicion pins colisionados
				//A
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, A.GetPosition(), A.GetRadius(), Color.green ) );
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, A.GetPosition(), A.GetPosition() + (B.GetPosition () - A.GetPosition()).normalized * A.GetRadius(), Color.green ) ); // Ra to Rb
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, A.GetPosition(), A.GetPosition() + (Solution1 - A.GetPosition()).normalized * A.GetRadius(), Color.green ) );// Linea A-Solution1
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, A.GetPosition(), A.GetPosition() + (Solution2 - A.GetPosition()).normalized * A.GetRadius(), Color.green ) );//Linea  B-Solution2
				//B
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, B.GetPosition(), B.GetRadius(), Color.green ) );
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, B.GetPosition(), B.GetPosition() + (A.GetPosition () - B.GetPosition()).normalized * B.GetRadius(), Color.green ) ); // Rb to Ra
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, B.GetPosition(), B.GetPosition() + (Solution1 - B.GetPosition()).normalized * B.GetRadius(), Color.green ) ); // Linea A-Solution1
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, B.GetPosition(), B.GetPosition() + (Solution2 - B.GetPosition()).normalized * B.GetRadius(), Color.green ) ); // Linea B-Solution2
				//Solution 1
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution1, newCircumference.GetRadius(), Color.yellow ) );
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution1, Solution1 + (A.GetPosition () - Solution1).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution1-A
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution1, Solution1 + (B.GetPosition () - Solution1).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution1-B
				//Solution 2
				DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, Solution2, newCircumference.GetRadius(), Color.yellow ) );			
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution2, Solution2 + (A.GetPosition () - Solution2).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution2-A
				DrawTheGizmo ( new GizmoToDraw( GizmoType.line, Solution2, Solution2 + (B.GetPosition () - Solution2).normalized * newCircumference.GetRadius(), Color.yellow ) ); // Linea Solution2-B
				//Posicion fianl decidida...
				//DrawTheGizmo ( new GizmoToDraw( GizmoType.sphere, newPin.transform.position, newCircumference.GetRadius(), Color.black ) );
				*/
			break;
			default:				
				Debug.Log("<color=red> ERROR WTF 111: nomero de colisiones incorrectas: </color>" + circumferencesCollided.Count.ToString());
			break;
		}
	}

	public void ProcessPin(Circumference newCircumference) {
		List<int> groupsCollided = new List<int>();
		for (int i = 0; i < circumferencesCollided.Count && !GameManager.instance.gameHasEnded; i++) {
			if (circumferencesCollided[i].tag == "Rotator") { // Si la colisión es con el rotator
				pinsGroups.Add(pinsGroups.Count, new PinsGroups(pinsGroups.Count, newCircumference));
				Debug.Log(string.Format("Se detectó colisión con Rotor. Creando nuevo grupo {0} </color>", pinsGroups.Count));
			}
			else {
				// Buscamos el grupo en el que ya esté el ultimo objeto que hemos tocado...
				int pinsGroupId = -1;
				for (int j = 0; j < pinsGroups.Count && pinsGroupId == -1; j++){
					if (pinsGroups[j].currentState == PinsGroups.GroupState.Active) {
						if ( pinsGroups[j].Contains(circumferencesCollided[i]) ) {
							pinsGroupId = j;
							Debug.Log(string.Format("Se detectó colisión con grupo: {0} </color>", j));
						}
					}
				}
				if (pinsGroupId != -1) {// ... Si hemos localizado un grupo en el que ya existe la circumferencia que evaluamos, metemos la nueva en ese grupo.
					Debug.Log(string.Format("<color=blue>Metemos newCircumference [{0}] en el grupo: {1} </color>",newCircumference, pinsGroupId));
					pinsGroups[pinsGroupId].AddMember(newCircumference);
					
					if (!groupsCollided.Contains(pinsGroupId)) {
						Debug.Log(string.Format("<color=yellow>Añadido Grupo Colisionado: </color>", pinsGroupId));
						groupsCollided.Add(pinsGroupId);
					}
				}
				else {// ... Si la circumferencia que evaluamos no pertenece a ningún grupo algo raro ha pasado y necesitamos un "salvoconducto"
					string log = "";
					foreach (var item in circumferencesCollided)
					{
						log += "\n - " + item.name;
					}
					Debug.Log ("<color=red>Error WTF(1): Los pins colisionados no están en ningún grupo. Esto no debería suceder</color> \n - Pin Evaluado: " + newCircumference.name + "\n - Pins Colisionados:" + log);

				}
			}
		}
		
		// Unificamos grupos si hemos colisionado con mas de uno
		if (groupsCollided.Count > 1) {
			int destiny = groupsCollided[0];
			for (int i = 1; i < groupsCollided.Count; i++) {
				int origin = groupsCollided[i];
				pinsGroups[origin].AddMembers(pinsGroups[destiny].members);
				pinsGroups[destiny].CombineWith(origin);
				Debug.Log(string.Format("<color=yellow>Combinados Grupos {0} y {1} </color>", destiny, origin));
			}
		}		
	}

	float ProcessPinsGroups() {	
		int totalPinsToDestroy = 0;	
		// Si encontramos un grupo de mas de dos miembros del mismo color...
		for(int i = 0; i < pinsGroups.Count; i++) {
			if (pinsGroups[i].currentState == PinsGroups.GroupState.Active){
				if (pinsGroups[i].Count > 2) {
					totalPinsToDestroy += pinsGroups[i].Count;
					// ...eliminamos el grupo.
					pinsGroups[i].Erase();
				}
			}
		}
		return minSpawnTime + (totalPinsToDestroy * Pin.TIME_TO_DESTROY);
	}

	float DistanceBetween(Vector3 A, Vector3 B) {
		return Mathf.Round( (B-A).sqrMagnitude * 100 ) / 100;
	}

	bool IsColliding(Circumference A, Circumference B, float margin = 0f) {
		return DistanceBetween( A.GetPosition(),B.GetPosition() ) < ( (A.GetRadius() + B.GetRadius() + margin) * (A.GetRadius() + B.GetRadius() + margin) );
	}


	/*   
	/// DEBUG 
	void PrintColorGroupsLog(string enunciado = "") {
		string log = enunciado.Length <= 0 ? "" : enunciado + ": \n";
		for(int i = 0; i < colorGroups.Count; i++) {
			for( int j = 0; j < colorGroups[i].Count; j++) {
				log += (colorGroups[i][j].name + " ");
			}
			log += "\r\n";
		}
		Debug.Log (log);
	}

	void DrawTheGizmo(GizmoToDraw g) {
		if (!gizmosToDraw.Contains(g))
			gizmosToDraw.Add(g);
	}

	void OnDrawGizmos() {
		foreach (GizmoToDraw gtd in gizmosToDraw) {
			switch (gtd.gizmoType) {
			case GizmoType.line:
				Gizmos.color = gtd.color;
				Gizmos.DrawLine(gtd.from, gtd.to);
				break;
			case GizmoType.sphere:
				Gizmos.color = gtd.color;
				Gizmos.DrawWireSphere(gtd.from, gtd.size);
				break;
			}
		}
	}
	*/
}
