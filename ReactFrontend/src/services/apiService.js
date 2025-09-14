import axios from "axios";

class ApiService {
  constructor() {
    // Use environment variable for API key (still visible in browser but not in git)
    this.apiKey = process.env.REACT_APP_API_KEY || 'your-api-secret-key-change-this';
    this.baseURL = "/api"; // Uses proxy from package.json
    this.token = null;
    this.tokenExpiry = null;

    // Create axios instance
    this.api = axios.create({
      baseURL: this.baseURL,
      timeout: 10000,
    });

    // Add request interceptor to include auth token
    this.api.interceptors.request.use(
      async (config) => {
        const token = await this.getValidToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Add response interceptor to handle token expiration
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (error.response?.status === 401) {
          // Token might be expired, try to refresh
          this.token = null;
          this.tokenExpiry = null;

          // Retry the original request once
          if (!error.config._retry) {
            error.config._retry = true;
            const newToken = await this.getValidToken();
            if (newToken) {
              error.config.headers.Authorization = `Bearer ${newToken}`;
              return this.api(error.config);
            }
          }
        }
        return Promise.reject(error);
      }
    );
  }

  async getValidToken() {
    // If we have a token that expires in more than 5 minutes, return it
    if (
      this.token &&
      this.tokenExpiry &&
      new Date(this.tokenExpiry) > new Date(Date.now() + 5 * 60 * 1000)
    ) {
      return this.token;
    }

    // Token is expired or about to expire, get a new one
    return await this.refreshToken();
  }

  async refreshToken() {
    try {
      console.log("Requesting new JWT token...");

      const response = await axios.post(
        `${this.baseURL}/auth/token`,
        {
          apiKey: this.apiKey,
        },
        {
          timeout: 10000,
        }
      );

      if (response.data && response.data.token) {
        this.token = response.data.token;
        this.tokenExpiry = response.data.expiresAt;

        console.log(
          "Successfully obtained JWT token, expires at:",
          response.data.expiresAt
        );
        return this.token;
      } else {
        console.error("Token response was null or empty");
        return null;
      }
    } catch (error) {
      console.error("Failed to get JWT token:", error);
      return null;
    }
  }

  // Blocked Users API
  async getBlockedUsers() {
    const response = await this.api.get("/blockedusers");
    return response.data;
  }

  async blockUser(userData) {
    const response = await this.api.post("/blockedusers", userData);
    return response.data;
  }

  async unblockUser(userId) {
    const response = await this.api.delete(`/blockedusers/${userId}`);
    return response.data;
  }

  // Timer API (for future use)
  async getTimers() {
    const response = await this.api.get("/timer");
    return response.data;
  }

  async getExpiredTimers() {
    const response = await this.api.get("/timer/expired");
    return response.data;
  }
}

// Create a singleton instance
const apiService = new ApiService();

export default apiService;
