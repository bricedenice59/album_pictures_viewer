importScripts('https://www.gstatic.com/firebasejs/8.8.0/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/8.8.0/firebase-messaging.js');

// Your web app's Firebase configuration
var firebaseConfig = {
    apiKey: "AIzaSyCxzMY4OPmJTJ3FNRt4GUKXx723tvItoiY",
    authDomain: "notifsethtx.firebaseapp.com",
    projectId: "notifsethtx",
    storageBucket: "notifsethtx.appspot.com",
    messagingSenderId: "848586593355",
    appId: "1:848586593355:web:1a3451890cdd62098ba9ce"
};

firebase.initializeApp(firebaseConfig);

// Retrieve firebase messaging
const messaging = firebase.messaging();

messaging.onBackgroundMessage(function (payload) {
    console.log('Received background message ', payload);

    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
    };

    self.registration.showNotification(notificationTitle, notificationOptions);
});