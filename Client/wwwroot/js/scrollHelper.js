window.scrollHelper = {
    scrollToEnd: function (element) {
        if (!element) return;
        element.scrollTop = element.scrollHeight;
    },

    isNearBottom: function (element) {
        if (!element) return true;
        const threshold = 80; // px from bottom
        const distance = element.scrollHeight - element.scrollTop - element.clientHeight;
        return distance < threshold;
    }
};
