<?php
/**
 * TastyIgniter Bridge
 * يربط نظامك مع TastyIgniter API
 * ضع هذا الملف في مجلد مشروعك على GitHub
 */

class TastyIgniterBridge {

    private string $baseUrl;
    private string $apiToken;
    private array $headers;

    public function __construct(string $baseUrl, string $apiToken) {
        // مثال: http://localhost/tastyigniter
        $this->baseUrl  = rtrim($baseUrl, '/');
        $this->apiToken = $apiToken;
        $this->headers  = [
            'Accept: application/json',
            'Content-Type: application/json',
            "Authorization: Bearer {$this->apiToken}",
        ];
    }

    // ─────────────────────────────────────────────
    //  MENUS - جلب قائمة الطعام
    // ─────────────────────────────────────────────

    /** جلب كل الأصناف */
    public function getMenus(int $locationId = 0): array {
        $url = $this->baseUrl . '/api/menus';
        if ($locationId) {
            $url .= "?location_id={$locationId}";
        }
        return $this->get($url);
    }

    /** جلب صنف واحد */
    public function getMenu(int $menuId): array {
        return $this->get($this->baseUrl . "/api/menus/{$menuId}");
    }

    // ─────────────────────────────────────────────
    //  CATEGORIES - الفئات
    // ─────────────────────────────────────────────

    public function getCategories(): array {
        return $this->get($this->baseUrl . '/api/categories');
    }

    // ─────────────────────────────────────────────
    //  LOCATIONS - الفروع
    // ─────────────────────────────────────────────

    public function getLocations(): array {
        return $this->get($this->baseUrl . '/api/locations');
    }

    // ─────────────────────────────────────────────
    //  ORDERS - الطلبات
    // ─────────────────────────────────────────────

    /**
     * إنشاء طلب جديد
     *
     * @param array $orderData بيانات الطلب
     * مثال:
     * [
     *   'first_name'   => 'أحمد',
     *   'last_name'    => 'محمد',
     *   'email'        => 'ahmed@example.com',
     *   'telephone'    => '0791234567',
     *   'order_type'   => 'delivery',   // أو 'collection'
     *   'location_id'  => 1,
     *   'payment'      => 'cod',        // الدفع عند الاستلام
     *   'comment'      => 'ملاحظات الطلب',
     *   'menu_items'   => [
     *       ['menu_id' => 1, 'quantity' => 2, 'comment' => ''],
     *       ['menu_id' => 3, 'quantity' => 1, 'comment' => 'بدون بصل'],
     *   ],
     * ]
     */
    public function createOrder(array $orderData): array {
        return $this->post($this->baseUrl . '/api/orders', $orderData);
    }

    /** جلب حالة طلب بواسطة ID */
    public function getOrder(int $orderId): array {
        return $this->get($this->baseUrl . "/api/orders/{$orderId}");
    }

    /** جلب كل الطلبات (للمطعم) */
    public function getOrders(array $filters = []): array {
        $url = $this->baseUrl . '/api/orders';
        if (!empty($filters)) {
            $url .= '?' . http_build_query($filters);
        }
        return $this->get($url);
    }

    /** تحديث حالة طلب */
    public function updateOrderStatus(int $orderId, int $statusId): array {
        return $this->patch(
            $this->baseUrl . "/api/orders/{$orderId}",
            ['status_id' => $statusId]
        );
    }

    // ─────────────────────────────────────────────
    //  CUSTOMERS - الزبائن
    // ─────────────────────────────────────────────

    /** إنشاء زبون جديد */
    public function createCustomer(array $customerData): array {
        return $this->post($this->baseUrl . '/api/customers', $customerData);
    }

    /** جلب بيانات زبون */
    public function getCustomer(int $customerId): array {
        return $this->get($this->baseUrl . "/api/customers/{$customerId}");
    }

    // ─────────────────────────────────────────────
    //  HTTP HELPERS
    // ─────────────────────────────────────────────

    private function get(string $url): array {
        return $this->request('GET', $url);
    }

    private function post(string $url, array $data): array {
        return $this->request('POST', $url, $data);
    }

    private function patch(string $url, array $data): array {
        return $this->request('PATCH', $url, $data);
    }

    private function request(string $method, string $url, array $data = []): array {
        $ch = curl_init($url);

        curl_setopt_array($ch, [
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_HTTPHEADER     => $this->headers,
            CURLOPT_TIMEOUT        => 30,
        ]);

        if ($method === 'POST') {
            curl_setopt($ch, CURLOPT_POST, true);
            curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($data));
        } elseif ($method === 'PATCH') {
            curl_setopt($ch, CURLOPT_CUSTOMREQUEST, 'PATCH');
            curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($data));
        }

        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        $error    = curl_error($ch);
        curl_close($ch);

        if ($error) {
            return ['success' => false, 'error' => $error];
        }

        $decoded = json_decode($response, true);

        return [
            'success'   => $httpCode >= 200 && $httpCode < 300,
            'http_code' => $httpCode,
            'data'      => $decoded['data'] ?? $decoded ?? [],
            'raw'       => $decoded,
        ];
    }
}
