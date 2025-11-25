window.geolocationFunctions = {
    getCurrentPosition: function () {
        return new Promise((resolve, reject) => {
            if (!navigator.geolocation) {
                reject("Geolocation is not supported by your browser.");
            } else {
                navigator.geolocation.getCurrentPosition(
                    position => {
                        resolve({
                            latitude: position.coords.latitude,
                            longitude: position.coords.longitude,
                            accuracy: position.coords.accuracy,
                            altitude: position.coords.altitude,        // in meters, can be null
                            altitudeAccuracy: position.coords.altitudeAccuracy
                        });
                    },
                    error => reject(error.message)
                );
            }
        });
    }
};
