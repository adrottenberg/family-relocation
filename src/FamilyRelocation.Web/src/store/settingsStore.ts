import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { setUserTimezone } from '../utils/datetime';

interface UserSettings {
  timeZoneId: string;
}

interface SettingsState {
  settings: UserSettings;
  isLoaded: boolean;
  setTimezone: (timezone: string) => void;
  loadSettings: () => Promise<void>;
  updateTimezone: (timezone: string) => Promise<void>;
}

const DEFAULT_TIMEZONE = 'America/New_York';

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set, get) => ({
      settings: {
        timeZoneId: DEFAULT_TIMEZONE,
      },
      isLoaded: false,

      setTimezone: (timezone: string) => {
        setUserTimezone(timezone);
        set({
          settings: {
            ...get().settings,
            timeZoneId: timezone,
          },
        });
      },

      loadSettings: async () => {
        // TODO: When UserSettingsController is implemented, fetch from API
        // For now, just use the persisted value or default
        const currentTimezone = get().settings.timeZoneId || DEFAULT_TIMEZONE;
        setUserTimezone(currentTimezone);
        set({ isLoaded: true });
      },

      updateTimezone: async (timezone: string) => {
        // TODO: When UserSettingsController is implemented, save to API
        // For now, just update local state
        get().setTimezone(timezone);
      },
    }),
    {
      name: 'user-settings-storage',
      partialize: (state) => ({
        settings: state.settings,
      }),
    }
  )
);

// Initialize timezone on module load from persisted state
const initialState = useSettingsStore.getState();
if (initialState.settings?.timeZoneId) {
  setUserTimezone(initialState.settings.timeZoneId);
}
