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

		/* Domain check (neni moc potreba)
		try {
			isIframe = window.self !== window.top || window.location.hostname === "localhost";
			if (!isIframe) {
				window.location.href = "https://td.kristn.co.uk/";
				return false;
			}

			return true;
		} catch (e) {
			window.location.href = "https://td.kristn.co.uk/";
			return false;
		}
        */
	},
});
