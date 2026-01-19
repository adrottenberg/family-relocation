/**
 * Family Relocation CRM - Ant Design Theme Configuration
 * 
 * Design System v4 - Light Button Style
 * 
 * Usage:
 * 1. Import this in your main.tsx or App.tsx
 * 2. Wrap your app with ConfigProvider using this theme
 * 
 * Example:
 * import { ConfigProvider } from 'antd';
 * import { theme } from './theme/antd-theme';
 * 
 * <ConfigProvider theme={theme}>
 *   <App />
 * </ConfigProvider>
 */

import type { ThemeConfig } from 'antd';

// =============================================================================
// COLOR TOKENS
// =============================================================================

export const colors = {
  // Brand Colors (Green - from Logo)
  brand: {
    600: '#2d7a3a',
    500: '#3d9a4a',
    400: '#4db85a',
    300: '#7ed68b',
    200: '#b5e8bb',
    100: '#e8f7ea',
    50: '#f4fbf5',
  },

  // Primary Colors (Blue - for interactive elements)
  primary: {
    700: '#1e40af',
    600: '#2563eb',
    500: '#3b82f6',
    400: '#60a5fa',
    300: '#93c5fd',
    200: '#bfdbfe',
    150: '#d0e4fc', // Button background
    100: '#dbeafe',
    50: '#eff6ff',
  },

  // Neutral Colors
  neutral: {
    900: '#1a1d1a',
    800: '#2d302d',
    700: '#404340',
    600: '#5c605c',
    500: '#7a7e7a',
    400: '#9ca09c',
    300: '#c4c7c4',
    200: '#e2e4e2',
    100: '#f1f2f1',
    50: '#f8f9f8',
  },

  // Status Colors (Board Decision)
  status: {
    pending: '#f59e0b',
    pendingBg: '#fef3c7',
    approved: '#10b981',
    approvedBg: '#d1fae5',
    rejected: '#ef4444',
    rejectedBg: '#fee2e2',
    deferred: '#6366f1',
    deferredBg: '#e0e7ff',
    paused: '#8b5cf6',
    pausedBg: '#ede9fe',
  },

  // Stage Colors (Pipeline)
  stage: {
    submitted: '#3b82f6',
    submittedBg: '#dbeafe',
    hunting: '#f59e0b',
    huntingBg: '#fef3c7',
    contract: '#8b5cf6',
    contractBg: '#ede9fe',
    closed: '#10b981',
    closedBg: '#d1fae5',
  },
} as const;

// =============================================================================
// ANT DESIGN THEME CONFIG
// =============================================================================

export const theme: ThemeConfig = {
  token: {
    // Primary color (used for focus states, links, etc.)
    colorPrimary: colors.primary[700],
    colorPrimaryHover: colors.primary[600],
    colorPrimaryActive: colors.primary[700],
    colorPrimaryBg: colors.primary[50],
    colorPrimaryBgHover: colors.primary[100],
    colorPrimaryBorder: colors.primary[200],
    colorPrimaryBorderHover: colors.primary[300],
    colorPrimaryText: colors.primary[700],
    colorPrimaryTextHover: colors.primary[600],
    colorPrimaryTextActive: colors.primary[700],

    // Success color (brand green)
    colorSuccess: colors.status.approved,
    colorSuccessBg: colors.status.approvedBg,
    colorSuccessBorder: '#86efac',
    colorSuccessText: '#065f46',

    // Warning color
    colorWarning: colors.status.pending,
    colorWarningBg: colors.status.pendingBg,
    colorWarningBorder: '#fcd34d',
    colorWarningText: '#92400e',

    // Error color
    colorError: colors.status.rejected,
    colorErrorBg: colors.status.rejectedBg,
    colorErrorBorder: '#fecaca',
    colorErrorText: '#991b1b',

    // Info color
    colorInfo: colors.primary[500],
    colorInfoBg: colors.primary[50],
    colorInfoBorder: colors.primary[200],
    colorInfoText: colors.primary[700],

    // Text colors
    colorText: colors.neutral[800],
    colorTextSecondary: colors.neutral[600],
    colorTextTertiary: colors.neutral[500],
    colorTextQuaternary: colors.neutral[400],

    // Background colors
    colorBgContainer: '#ffffff',
    colorBgElevated: '#ffffff',
    colorBgLayout: colors.neutral[50],
    colorBgSpotlight: colors.neutral[100],

    // Border colors
    colorBorder: colors.neutral[300],
    colorBorderSecondary: colors.neutral[200],

    // Typography
    fontFamily: "'Assistant', 'Heebo', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    fontSize: 14,
    fontSizeHeading1: 30,
    fontSizeHeading2: 24,
    fontSizeHeading3: 20,
    fontSizeHeading4: 18,
    fontSizeHeading5: 16,
    fontSizeLG: 16,
    fontSizeSM: 13,
    fontSizeXL: 18,

    // Border radius
    borderRadius: 6,
    borderRadiusLG: 8,
    borderRadiusSM: 4,
    borderRadiusXS: 2,

    // Spacing
    padding: 16,
    paddingLG: 24,
    paddingSM: 12,
    paddingXS: 8,
    paddingXXS: 4,

    margin: 16,
    marginLG: 24,
    marginSM: 12,
    marginXS: 8,
    marginXXS: 4,

    // Shadows
    boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)',
    boxShadowSecondary: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',

    // Control height
    controlHeight: 36,
    controlHeightLG: 44,
    controlHeightSM: 28,

    // Line heights
    lineHeight: 1.5,
    lineHeightHeading1: 1.2,
    lineHeightHeading2: 1.3,
    lineHeightHeading3: 1.4,
  },

  components: {
    // ==========================================================================
    // BUTTON - Light style (Option B)
    // ==========================================================================
    Button: {
      // Primary button - light background with dark text
      colorPrimary: colors.primary[700],
      colorPrimaryHover: colors.primary[700],
      colorPrimaryActive: colors.primary[700],
      primaryColor: colors.primary[700], // Text color
      colorPrimaryBg: colors.primary[150], // Background
      defaultBg: '#ffffff',
      defaultBorderColor: colors.neutral[300],
      
      // Override primary button styles
      algorithm: true,
      
      // Border radius
      borderRadius: 6,
      borderRadiusLG: 8,
      borderRadiusSM: 4,

      // Font
      fontWeight: 600,
      
      // Padding
      paddingInline: 18,
      paddingInlineLG: 24,
      paddingInlineSM: 12,
    },

    // ==========================================================================
    // INPUT
    // ==========================================================================
    Input: {
      colorBgContainer: '#ffffff',
      colorBorder: colors.neutral[300],
      colorPrimaryHover: colors.primary[400],
      activeBorderColor: colors.primary[400],
      hoverBorderColor: colors.primary[300],
      activeShadow: `0 0 0 3px ${colors.primary[50]}`,
      borderRadius: 6,
      paddingInline: 12,
      paddingBlock: 8,
    },

    // ==========================================================================
    // SELECT
    // ==========================================================================
    Select: {
      colorBgContainer: '#ffffff',
      colorBorder: colors.neutral[300],
      borderRadius: 6,
      optionSelectedBg: colors.primary[50],
      optionActiveBg: colors.primary[50],
    },

    // ==========================================================================
    // TABLE
    // ==========================================================================
    Table: {
      headerBg: colors.neutral[50],
      headerColor: colors.neutral[500],
      headerSplitColor: colors.neutral[200],
      rowHoverBg: colors.primary[50],
      borderColor: colors.neutral[200],
      cellPaddingBlock: 16,
      cellPaddingInline: 16,
    },

    // ==========================================================================
    // CARD
    // ==========================================================================
    Card: {
      colorBgContainer: '#ffffff',
      colorBorderSecondary: colors.neutral[200],
      borderRadiusLG: 12,
      paddingLG: 24,
    },

    // ==========================================================================
    // TAG
    // ==========================================================================
    Tag: {
      borderRadiusSM: 4,
      defaultBg: colors.neutral[100],
      defaultColor: colors.neutral[600],
    },

    // ==========================================================================
    // MENU (Sidebar)
    // ==========================================================================
    Menu: {
      itemBg: 'transparent',
      itemColor: colors.neutral[600],
      itemHoverBg: colors.neutral[100],
      itemHoverColor: colors.neutral[900],
      itemSelectedBg: colors.primary[50],
      itemSelectedColor: colors.primary[700],
      itemBorderRadius: 8,
      iconSize: 18,
      itemMarginInline: 8,
      itemPaddingInline: 14,
    },

    // ==========================================================================
    // TABS
    // ==========================================================================
    Tabs: {
      inkBarColor: colors.primary[600],
      itemSelectedColor: colors.primary[700],
      itemHoverColor: colors.primary[600],
      itemColor: colors.neutral[500],
    },

    // ==========================================================================
    // MODAL
    // ==========================================================================
    Modal: {
      borderRadiusLG: 16,
      paddingContentHorizontalLG: 24,
      titleFontSize: 20,
    },

    // ==========================================================================
    // FORM
    // ==========================================================================
    Form: {
      labelColor: colors.neutral[700],
      labelFontSize: 14,
    },

    // ==========================================================================
    // STATISTIC (Dashboard Cards)
    // ==========================================================================
    Statistic: {
      titleFontSize: 13,
      contentFontSize: 24,
    },

    // ==========================================================================
    // LAYOUT
    // ==========================================================================
    Layout: {
      headerBg: '#ffffff',
      headerColor: colors.neutral[800],
      bodyBg: colors.neutral[50],
      siderBg: '#ffffff',
    },

    // ==========================================================================
    // TOOLTIP
    // ==========================================================================
    Tooltip: {
      colorBgSpotlight: colors.neutral[900],
      borderRadius: 6,
    },

    // ==========================================================================
    // MESSAGE / NOTIFICATION
    // ==========================================================================
    Message: {
      borderRadiusLG: 8,
    },

    Notification: {
      borderRadiusLG: 12,
    },
  },
};

// =============================================================================
// CSS VARIABLES (for custom components)
// =============================================================================

export const cssVariables = `
:root {
  /* Brand Colors */
  --brand-600: ${colors.brand[600]};
  --brand-500: ${colors.brand[500]};
  --brand-400: ${colors.brand[400]};
  --brand-300: ${colors.brand[300]};
  --brand-200: ${colors.brand[200]};
  --brand-100: ${colors.brand[100]};
  --brand-50: ${colors.brand[50]};

  /* Primary Colors */
  --primary-700: ${colors.primary[700]};
  --primary-600: ${colors.primary[600]};
  --primary-500: ${colors.primary[500]};
  --primary-400: ${colors.primary[400]};
  --primary-300: ${colors.primary[300]};
  --primary-200: ${colors.primary[200]};
  --primary-150: ${colors.primary[150]};
  --primary-100: ${colors.primary[100]};
  --primary-50: ${colors.primary[50]};

  /* Neutral Colors */
  --neutral-900: ${colors.neutral[900]};
  --neutral-800: ${colors.neutral[800]};
  --neutral-700: ${colors.neutral[700]};
  --neutral-600: ${colors.neutral[600]};
  --neutral-500: ${colors.neutral[500]};
  --neutral-400: ${colors.neutral[400]};
  --neutral-300: ${colors.neutral[300]};
  --neutral-200: ${colors.neutral[200]};
  --neutral-100: ${colors.neutral[100]};
  --neutral-50: ${colors.neutral[50]};

  /* Status Colors */
  --status-pending: ${colors.status.pending};
  --status-pending-bg: ${colors.status.pendingBg};
  --status-approved: ${colors.status.approved};
  --status-approved-bg: ${colors.status.approvedBg};
  --status-rejected: ${colors.status.rejected};
  --status-rejected-bg: ${colors.status.rejectedBg};
  --status-deferred: ${colors.status.deferred};
  --status-deferred-bg: ${colors.status.deferredBg};

  /* Stage Colors */
  --stage-submitted: ${colors.stage.submitted};
  --stage-submitted-bg: ${colors.stage.submittedBg};
  --stage-hunting: ${colors.stage.hunting};
  --stage-hunting-bg: ${colors.stage.huntingBg};
  --stage-contract: ${colors.stage.contract};
  --stage-contract-bg: ${colors.stage.contractBg};
  --stage-closed: ${colors.stage.closed};
  --stage-closed-bg: ${colors.stage.closedBg};

  /* Typography */
  --font-family: 'Assistant', 'Heebo', -apple-system, BlinkMacSystemFont, sans-serif;

  /* Shadows */
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  --shadow-lg: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05);

  /* Border Radius */
  --radius-sm: 4px;
  --radius-md: 6px;
  --radius-lg: 8px;
  --radius-xl: 12px;
}
`;

// =============================================================================
// STATUS TAG STYLES (for custom Tag components)
// =============================================================================

export const statusTagStyles = {
  pending: {
    backgroundColor: colors.status.pendingBg,
    color: '#92400e',
    border: 'none',
  },
  approved: {
    backgroundColor: colors.status.approvedBg,
    color: '#065f46',
    border: 'none',
  },
  rejected: {
    backgroundColor: colors.status.rejectedBg,
    color: '#991b1b',
    border: 'none',
  },
  deferred: {
    backgroundColor: colors.status.deferredBg,
    color: '#3730a3',
    border: 'none',
  },
};

export const stageTagStyles = {
  submitted: {
    backgroundColor: colors.stage.submittedBg,
    color: '#1e40af',
    border: 'none',
  },
  houseHunting: {
    backgroundColor: colors.stage.huntingBg,
    color: '#92400e',
    border: 'none',
  },
  underContract: {
    backgroundColor: colors.stage.contractBg,
    color: '#5b21b6',
    border: 'none',
  },
  closed: {
    backgroundColor: colors.stage.closedBg,
    color: '#065f46',
    border: 'none',
  },
};

// =============================================================================
// USAGE EXAMPLE
// =============================================================================

/*
// main.tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import { ConfigProvider } from 'antd';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { theme, cssVariables } from './theme/antd-theme';
import App from './App';

// Inject CSS variables
const style = document.createElement('style');
style.textContent = cssVariables;
document.head.appendChild(style);

const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ConfigProvider theme={theme}>
        <App />
      </ConfigProvider>
    </QueryClientProvider>
  </React.StrictMode>
);
*/

export default theme;
