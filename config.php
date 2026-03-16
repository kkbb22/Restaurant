<?php
/**
 * إعدادات الربط مع TastyIgniter
 * ──────────────────────────────
 * 1. افتح TastyIgniter Admin
 * 2. روح على: Tools > APIs > Create Token
 * 3. انسخ التوكن وحطه هون
 */

return [

    // رابط موقع TastyIgniter الخاص بك
    'base_url' => 'http://localhost/tastyigniter',

    // توكن API - احصل عليه من Admin > Tools > APIs
    'api_token' => '1|Df0L8hycMtC4V173Ll8EQPUrM5q9MQLd9GZoIzuHALpxQVIAxsBZaL8FPX0h58rdjmRquFGqUeFwbG9r',

    // رقم الفرع الافتراضي (location_id)
    'default_location_id' => 1,

    // طريقة الدفع الافتراضية
    'default_payment' => 'cod', // cod = الدفع عند الاستلام

];
