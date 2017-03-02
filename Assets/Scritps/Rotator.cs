using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : Circumference {

	public float speed = 100f;
	public float minSpawnTime = 0.2f;

	private float spawnTimeDelay;

	Circumference me;

	private List<Circumference> childPins = new List<Circumference>();
	private List<Circumference> circumferencesCollided = new List<Circumference>();
	private List<List<Circumference>> colorGroups = new List<List<Circumference>>();

	//private List<GizmoToDraw> gizmosToDraw = new List<GizmoToDraw>();

	public override void Initialize() {
		childPins.Clear();
		colorGroups.Clear();
		me = GetComponent<Circumference>();
	}

	void Update() {
		transform.Rotate(0f, 0f, speed * Time.deltaTime);
	}

	public void AddPin(Circumference newPin, Collider2D col) {
		
		SearchNearestPins(newPin);
		Pin cn =newPin.GetComponent<Pin>();
		cn.cc.isTrigger = false;
		cn.isPinned = true;
		newPin.transform.SetParent(transform);
		
		if(!GameManager.instance.gameHasEnded) {
			Reposition(newPin);
			EvaluatePin(newPin);		
		
			if (!childPins.Contains(newPin)) childPins.Add(newPin);

			spawnTimeDelay = EvaluateColorGroups();
			GameManager.instance.CheckDifficulty();
			GameManager.instance.spawner.SpawnPin(spawnTimeDelay);
		}
	}

	void SearchNearestPins(Circumference newCircumference) {
		circumferencesCollided.Clear();
		// Comprobamos la distancia con el resto de bolas
		foreach (Circumference son in childPins) {
			if (son.transform.parent.tag == "Rotator")
				if ( IsColliding(newCircumference, son, 0.02f) ) {
					if (son.name.Split('-')[1] !=  newCircumference.name.Split('-')[1]) {
						GameManager.instance.EndGame();
						break;
					}
					circumferencesCollided.Add (son);
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
				float Lc = A.GetRadius() + B.GetRadius();
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

	public void EvaluatePin(Circumference newCircumference) {
		List<int> groupsCollided = new List<int>();
		for (int i = 0; i < circumferencesCollided.Count && !GameManager.instance.gameHasEnded; i++) {
			if (circumferencesCollided[i].tag == "Rotator") { // Si la colisión es con el rotator
				CreateColorList(newCircumference);
			}
			/*else if ( newCircumference.name.Split('-')[1] != circumferencesCollided[i].name.Split('-')[1] ){// Si la colisión es con una burbuja de distinto color
				GameManager.instance.EndGame();
				continue;
				//CreateColorGroup(newCircumference);
			}*/
			else {
				// Buscamos el grupo en el que ya esté el ultimo objeto que hemos tocado...
				int colorGroupId = -1;
				try {
					for (int j = 0; j < colorGroups.Count && colorGroupId == -1; j++){
						if ( colorGroups[j].Contains(circumferencesCollided[i]) ) {
							colorGroupId = j;
						}
					}
				} catch (MissingReferenceException mre) {
					Debug.Log("<color=red> EXCEPTION: " + mre.Data + "</color>");
				}

				if (colorGroupId == -1) {// ... Si la circumferencia que evaluamos no pertenece a ningún grupo algo raro ha pasado y necesitamos un "salvoconducto"
					groupsCollided.Add(CreateColorList(newCircumference)); // "salvoconducto" - Creamos un nuevo grupo para no detener el juego... pero es un error
					string log = "";
					foreach (var item in circumferencesCollided)
					{
						log += "\n - " + item.name;
					}
					Debug.Log ("<color=red>Error WTF(1): Esto no debería suceder</color> \n - Pin Evaluado: " + newCircumference.name + "\n - Pins Colisionados:" + log);

				}
				else{// ... Si hemos localizado un grupo en el que ya existe la circumferencia que evaluamos, metemos la nueva en ese grupo.
					colorGroups[colorGroupId].Add(newCircumference);
					if (!groupsCollided.Contains(colorGroupId)) groupsCollided.Add(colorGroupId);
				}
			}
		}

		/*
		// unificamos grupos si hemos colisionado con mas de uno
		if (groupsCollided.Count > 1) {
			for (int i = 1; i < groupsCollided.Count; i++) {
				int id = groupsCollided[i];
				foreach (Circumference c in colorGroups[id]) {
					if (!colorGroups[groupsCollided[0]].Contains(c))
						colorGroups[groupsCollided[0]].Add(c);
				}
				colorGroups.RemoveAt(i);
			}
		}
		*/
	}

	float EvaluateColorGroups() {
		List<int> groupsToDestroy = new List<int>();		
		// Si encontramos un grupo de mas de dos miembros del mismo color...
		for(int i = 0; i < colorGroups.Count; i++) {
			if (colorGroups[i].Count > 2) {
				// ...eliminamos el grupo.
				groupsToDestroy.Add(i);
			}
		}
		// Una vez detectados los grupos a destruir les damos orden de destruir
		if (groupsToDestroy.Count > 0){
			int totalPinsToDestroy = 0;
			foreach(int i in groupsToDestroy) {
				// ...los objetos que contiene...
				totalPinsToDestroy += colorGroups[i].Count;
				StartCoroutine(DestroyElementsFromGroup(colorGroups[i]));				
				colorGroups.RemoveAt(i);
				GameManager.instance.Score++;
			}
			// Aumento un poco el tiempo de respawn tras las eliminaciones
			return totalPinsToDestroy * Pin.TIME_TO_DESTROY;
		}
		return minSpawnTime;
	}
	
	int CreateColorList(Circumference newCircumference) {
		List<Circumference> cList = new List<Circumference>();
		cList.Add(newCircumference);
		return CreateListGroup(cList);
	}
	
	int CreateListGroup(List<Circumference> gos) {
		List<Circumference> goList = new List<Circumference>(gos);
		colorGroups.Add(goList);
		return colorGroups.Count;
	}
	
	IEnumerator DestroyElementsFromGroup(List<Circumference> circumferences) {
		for( int i = 0; i < circumferences.Count; i++) {
			childPins.Remove(circumferences[i]);
			circumferences[i].gameObject.GetComponent<Pin>().Autodestroy();
			yield return new WaitForSeconds(Pin.TIME_TO_DESTROY/circumferences.Count);
		}
	}

	float DistanceBetween(Vector3 A, Vector3 B) {
		return Mathf.Round( (B-A).sqrMagnitude * 100 ) / 100;
	}

	bool IsColliding(Circumference A, Circumference B, float margin = 0f) {
		return DistanceBetween( A.GetPosition(),B.GetPosition() ) < ( (A.GetRadius() + B.GetRadius() + margin) * (A.GetRadius() + B.GetRadius() + margin) );

	}

	/*
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
