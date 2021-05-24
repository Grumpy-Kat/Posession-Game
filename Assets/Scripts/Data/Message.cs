using UnityEngine;

public enum MessageType { Attack, Possession, Default }

public class Message  {
	public MessageType type { get; protected set; }
	public Vector3 src { get; protected set; }

	public Message(MessageType type, Vector3 src) {
		this.type = type;
		this.src = src;
	}
}

