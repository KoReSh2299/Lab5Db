class TokenHandler {
    constructor(storageKey = 'jwtToken') {
        this.storageKey = storageKey;
    }

    saveToken(token) {
        localStorage.setItem(this.storageKey, token);
    }

    getToken() {
        return localStorage.getItem(this.storageKey);
    }

    removeToken() {
        localStorage.removeItem(this.storageKey);
    }

    isTokenExpired() {
        const token = this.getToken();
        if (!token) return true;

        const payload = JSON.parse(atob(token.split('.')[1]));
        const now = Math.floor(Date.now() / 1000);
        return payload.exp < now;
    }

    setAuthHeader(fetchOptions = {}) {
        const token = this.getToken();
        if (!token || this.isTokenExpired()) {
            console.warn('Token is missing or expired.');
            return fetchOptions;
        }

        if (!fetchOptions.headers) {
            fetchOptions.headers = {};
        }
        fetchOptions.headers['Authorization'] = `Bearer ${token}`;
        return fetchOptions;
    }
}

const tokenHandler = new TokenHandler();
