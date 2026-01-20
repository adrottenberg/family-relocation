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
  setTokens: (tokens: AuthTokens) => void;
  setUser: (user: User) => void;
  logout: () => void;
  canApproveBoardDecisions: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      tokens: null,
      user: null,
      isAuthenticated: false,

      setTokens: (tokens) =>
        set({
          tokens,
          isAuthenticated: true,
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
        }),

      canApproveBoardDecisions: () => {
        const user = get().user;
        if (!user?.roles) return false;
        return user.roles.some(role =>
          role === 'Admin' || role === 'BoardMember'
        );
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        tokens: state.tokens,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
