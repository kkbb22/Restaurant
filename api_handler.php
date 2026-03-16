<?php
/**
 * api_handler.php
 * نقطة الاتصال بين واجهة الزبون ونظام TastyIgniter
 * ضع هذا الملف في مجلد مشروعك
 */

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

require_once __DIR__ . '/TastyIgniterBridge.php';

$config = require __DIR__ . '/config.php';
$bridge = new TastyIgniterBridge($config['base_url'], $config['api_token']);

$action = $_GET['action'] ?? '';

switch ($action) {

    // ─── جلب قائمة الطعام ───────────────────────
    case 'get_menus':
        $locationId = (int)($_GET['location_id'] ?? $config['default_location_id']);
        $result = $bridge->getMenus($locationId);
        echo json_encode($result);
        break;

    // ─── جلب الفئات ────────────────────────────
    case 'get_categories':
        $result = $bridge->getCategories();
        echo json_encode($result);
        break;

    // ─── إنشاء طلب جديد ────────────────────────
    case 'create_order':
        if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
            http_response_code(405);
            echo json_encode(['success' => false, 'error' => 'POST required']);
            break;
        }

        $input = json_decode(file_get_contents('php://input'), true);

        if (!$input) {
            http_response_code(400);
            echo json_encode(['success' => false, 'error' => 'بيانات غير صحيحة']);
            break;
        }

        // التحقق من الحقول المطلوبة
        $required = ['first_name', 'telephone', 'menu_items'];
        foreach ($required as $field) {
            if (empty($input[$field])) {
                http_response_code(400);
                echo json_encode(['success' => false, 'error' => "الحقل {$field} مطلوب"]);
                exit;
            }
        }

        // بناء بيانات الطلب
        $orderData = [
            'first_name'  => $input['first_name'],
            'last_name'   => $input['last_name'] ?? '',
            'email'       => $input['email'] ?? '',
            'telephone'   => $input['telephone'],
            'order_type'  => $input['order_type'] ?? 'collection',
            'location_id' => $input['location_id'] ?? $config['default_location_id'],
            'payment'     => $input['payment'] ?? $config['default_payment'],
            'comment'     => $input['comment'] ?? '',
            'menu_items'  => $input['menu_items'],
        ];

        $result = $bridge->createOrder($orderData);
        echo json_encode($result);
        break;

    // ─── تتبع طلب ──────────────────────────────
    case 'get_order':
        $orderId = (int)($_GET['order_id'] ?? 0);
        if (!$orderId) {
            http_response_code(400);
            echo json_encode(['success' => false, 'error' => 'رقم الطلب مطلوب']);
            break;
        }
        $result = $bridge->getOrder($orderId);
        echo json_encode($result);
        break;

    // ─── جلب الفروع ────────────────────────────
    case 'get_locations':
        $result = $bridge->getLocations();
        echo json_encode($result);
        break;

    default:
        http_response_code(400);
        echo json_encode(['success' => false, 'error' => 'action غير معروف']);
}
