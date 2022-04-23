using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Console.Scripts;
using Mirror;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEditor;
using UnityEngine;

// Todo: this only supports having one weapon one one bone at the moment
public class WeaponController : NetworkBehaviour {

	[Header("References")]
	public GameObject weaponPart;
	private GameObject meshHolder;
	
	[Space]
	public Bones bones;

	[Space]
	[SerializeField]
	private Stats stats;

	public Weapon weapon;

	public PlayerController skillUser;

	/// <summary>
	/// Main method for building the weapon on the weapon controller
	/// </summary>
	/// <param name="newWeapon">The weapon to build</param>
	///
	
	[Command]
	public void CmdBuildWeapon(Weapon newWeapon) {
		Clear();

		BuildWeaponMesh(newWeapon);
	}

	public GameObject BuildWeaponMesh(Weapon newWeapon)
	{
		//Cache info
		weapon = newWeapon;
		//stats = weapon.stats;

		meshHolder = new GameObject("Mesh Holder");

		var bone = bones.ContainsKey(newWeapon.mountInformation.bone) ? newWeapon.mountInformation.bone : "default";
		
		meshHolder.transform.parent = bones[bone];
		meshHolder.transform.localPosition = newWeapon.mountInformation.position;
		meshHolder.transform.localRotation = Quaternion.Euler(newWeapon.mountInformation.rotation);

		
		var listTemplates = new List<WeaponPartTemplate>();
		foreach(string partname in weapon.partNames) {
			//WeaponPartTemplate part = Resources.Load<WeaponPartTemplate>("test/"+partname);
			WeaponPartTemplate part = WeaponGenerator.LoadWeaponPart(partname);
			
			listTemplates.Add(part);
			GameObject weaponPartObj = Instantiate(weaponPart, meshHolder.transform);
			//weaponPart.transform.localPosition = meshHolder.transform.localPosition;
			weaponPartObj.name = partname;
			weaponPartObj.GetComponent<MeshFilter>().mesh = part.mesh.GetComponent<MeshFilter>().sharedMesh;
			weaponPartObj.GetComponent<MeshRenderer>().material = part.mesh.GetComponent<MeshRenderer>().sharedMaterial;
		}
		meshHolder.transform.GetChild(0).localPosition = Vector3.zero;
		
		for(var i = 0; i < listTemplates.Count - 1; i++) {
			try {
				var nextConnection = listTemplates[i].mesh.transform
					.Cast<Transform>()
					.Where(firstPart => listTemplates[i + 1].mesh.transform
						.Cast<Transform>()
						.Any(secondPart => secondPart.name == firstPart.name))
					.Select(partTransform => partTransform.name)
					.First();

				var firstPoint = listTemplates[i].mesh.transform.Find(nextConnection);
				var secondPoint = listTemplates[i + 1].mesh.transform.Find(nextConnection);

				meshHolder.transform.GetChild(i + 1).localRotation = Quaternion.identity;
				meshHolder.transform.GetChild(i + 1).Rotate(-firstPoint.localRotation.eulerAngles);
				meshHolder.transform.GetChild(i + 1).Rotate(secondPoint.localRotation.eulerAngles);
				//GameConsole.Log(meshHolder.transform.GetChild(i + 1).position);
				meshHolder.transform.GetChild(i + 1).localPosition = (meshHolder.transform.GetChild(i).localPosition + firstPoint.localPosition) - secondPoint.localPosition;
				//GameConsole.Log(meshHolder.transform.GetChild(i + 1).position);
			} catch(ArgumentNullException ex) {
				GameConsole.Error($"Could not find connection between {listTemplates[i]} and {listTemplates[i + 1]}");
			}
		}

		return meshHolder;
	}

	[ContextMenu("Clear")]
	public void Clear() {
		//Destroy all of the mesh holder's children
		//foreach (Transform child in meshHolder.transform) {
		//	Destroy(child.gameObject);
		//}
		Destroy(meshHolder);
		meshHolder = null;

		if (weapon != null)
		{
			var spawningModifiers = weapon.modifiers.Where(modifier => modifier is IWeaponBuildModifier)
				.Cast<IWeaponBuildModifier>();
			foreach (var spawningModifier in spawningModifiers)
			{
				spawningModifier.OnWeaponClear(this, weapon);
			}
		}

		//Cache info
		weapon = null;
		stats = null;
	}

	[Obsolete]
	public float CalculateStat(string statName) {
		var statValue = 0f;

		return statValue;
	}

}

[Serializable]
public class Bones : SerializableDictionaryBase<string, Transform> { }
