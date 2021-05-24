using UnityEngine;

public class MessageAttack : Message {
	public float range { get; protected set; }
	public bool requiresSightline { get; protected set; }

	public MessageAttack(Vector3 src, float range, bool requiresSightline) : base(MessageType.Attack, src) {
		this.range = range;
		this.requiresSightline = requiresSightline;
	}
}

