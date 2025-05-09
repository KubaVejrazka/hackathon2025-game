mergeInto(LibraryManager.library, {
	SendMessageToJS: function (data) {
		try {
			const message = JSON.parse(UTF8ToString(data));

			window.parent.postMessage(
				{
					type: "unityToJs",
					data: message,
				},
				"*"
			);
		} catch (e) {
			console.error("Error processing message from Unity:", e);
		}
	},
	InitMessageListener: function () {
		console.log('initializing message listener');
		if (typeof window !== "undefined") {
			window.addEventListener("message", function (event) {
				if (event.data.type !== "jsToUnity") return;

				try {
					const message = JSON.stringify(event.data.data);
					SendMessage("BrowserMessanger", "_ReceiveFromJavaScript", message);
				} catch (e) {
					console.error("Error sending message to Unity:", e);
				}
			});
		}
		return true;
	},
});
