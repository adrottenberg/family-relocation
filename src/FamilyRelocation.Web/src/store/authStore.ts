import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthTokens {
  accessToken: string;
  idToken: string;
  refreshToken: string;
  expiresIn: number;
}

interface User {
  email: string;
  name?: string;
  roles?: string[];
}

interface AuthState {
  tokens: AuthTokens | null;
  user: User | null;
  isAuthenticated: boolean;
  rolesFetched: boolean;
  setTokens: (tokens: AuthTokens) => void;
  setUser: (user: User) => void;
  logout: () => void;
  canApproveBoardDecisions: () => boolean;
  fetchAndSetRoles: () => Promise<void>;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      tokens: null,
      user: null,
      isAuthenticated: false,
      rolesFetched: false,

      setTokens: (tokens) =>
        set({
          tokens,
          isAuthenticated: true,
          rolesFetched: false, // Reset so roles are fetched fresh after login
        }),

      setUser: (user) =>
        set({
          user,
        }),

      logout: () =>
        set({
          tokens: null,
          user: null,
          isAuthenticated: false,
          rolesFetched: false,
        }),

      canApproveBoardDecisions: () => {
        const user = get().user;
        if (!user?.roles) return false;
        return user.roles.some(role =>
          role === 'Admin' || role === 'BoardMember'
        );
      },

      fetchAndSetRoles: async () => {
        const { isAuthenticated, user, rolesFetched } = get();
        if (!isAuthenticated || !user || rolesFetched) return;

        try {
          // Import dynamically to avoid circular dependency
          const { authApi } = await import('../api');

          // Fetch actual roles from database
          const rolesResponse = await authApi.getMyRoles();

          set({
            user: { ...user, roles: rolesResponse.roles },
            rolesFetched: true,
          });
        } catch (error) {
          console.error('Failed to fetch roles:', error);
          set({ rolesFetched: true }); // Mark as fetched to prevent retry loop
        }
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        tokens: state.tokens,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
        // Don't persist rolesFetched - we want to fetch fresh roles on page refresh
      }),
    }
  )
);
