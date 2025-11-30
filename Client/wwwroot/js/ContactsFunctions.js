console.log("contacts.js loaded");

window.contactsFunctions = {
isSupported: function() {
        console.log(navigator);
        console.log(window);
        return ("contacts" in navigator) && ("ContactsManager" in window);
    },

    getAvailableProperties: async function()
    {
        if (!this.isSupported()) return [];
        if (navigator.contacts.getProperties)
        {
            return await navigator.contacts.getProperties();
        }
        return [];
    },

    isAndroidChrome: function() {
        const ua = navigator.userAgent.toLowerCase();
        return ua.includes("android") && ua.includes("chrome");
    },

    pickContacts: async function(props = ["name", "tel"], opts = { multiple: false }) {
        if (!this.isSupported()) throw "Contact Picker API not supported on this device/browser.";
        try
        {
            const contacts = await navigator.contacts.select(props, opts);
            return contacts;
        }
        catch (err)
        {
            throw err && err.message ? err.message : String(err);
        }
    }
}
;
