// ESLint flat config for the Theme Angular project.
// Enforces the rules in .editorconfig (via eslint-plugin-editorconfig, which reads the
// file per source file), the Angular and TypeScript rules, and the Angular template rules.
// SonarCloud reads the lint report produced by `npm run lint:report` (see sonar-project.properties).
import eslint from '@eslint/js';
import tseslint from 'typescript-eslint';
import angular from 'angular-eslint';
import editorconfig from 'eslint-plugin-editorconfig';
import eslintConfigPrettier from 'eslint-config-prettier';

export default tseslint.config(
  {
    ignores: ['dist/', '.angular/', 'node_modules/', 'coverage/', 'eslint-report.json'],
  },

  // TypeScript sources
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.strictTypeChecked,
      tseslint.configs.stylisticTypeChecked,
      angular.configs.tsAll,
    ],
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    plugins: { editorconfig },
    processor: angular.processInlineTemplates,
    rules: {
      // .editorconfig: root, [*], [*.ts] (indent 2 spaces, UTF-8, final newline, no trailing spaces)
      'editorconfig/charset': 'error',
      'editorconfig/eol-last': 'error',
      'editorconfig/indent': 'error',
      'editorconfig/no-trailing-spaces': 'error',
      'editorconfig/linebreak-style': 'error',
      // .editorconfig [*.ts] quote_type = single
      quotes: ['error', 'single', { avoidEscape: true }],
      // Angular: prefix from angular.json is "app"
      '@angular-eslint/component-selector': [
        'error',
        { type: 'element', prefix: 'app', style: 'kebab-case' },
      ],
      '@angular-eslint/directive-selector': [
        'error',
        { type: 'attribute', prefix: 'app', style: 'camelCase' },
      ],
      // Components keep templates and styles in external files (AGENTS.md ENG-UI-18)
      '@angular-eslint/prefer-on-push-component-change-detection': 'error',
      '@angular-eslint/component-max-inline-declarations': [
        'error',
        { template: 0, styles: 0 },
      ],
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-floating-promises': 'error',
      '@typescript-eslint/consistent-type-definitions': ['error', 'interface'],
      '@typescript-eslint/explicit-member-accessibility': ['error', { accessibility: 'explicit' }],
      eqeqeq: ['error', 'always'],
      curly: ['error', 'all'],
      'no-console': ['warn', { allow: ['warn', 'error'] }],
    },
  },

  // Angular component templates
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateAll],
    plugins: { editorconfig },
    rules: {
      'editorconfig/charset': 'error',
      'editorconfig/eol-last': 'error',
      'editorconfig/indent': 'error',
      'editorconfig/no-trailing-spaces': 'error',
      'editorconfig/linebreak-style': 'error',
      // Templates must not use inline styles (styles live in external .css files)
      '@angular-eslint/template/no-inline-styles': 'error',
    },
  },

  // Plain JS/MJS/JSON-like config files: editorconfig only
  {
    files: ['**/*.{js,mjs,cjs}'],
    plugins: { editorconfig },
    rules: {
      'editorconfig/charset': 'error',
      'editorconfig/eol-last': 'error',
      'editorconfig/indent': 'error',
      'editorconfig/no-trailing-spaces': 'error',
      'editorconfig/linebreak-style': 'error',
    },
  },

  // Turn off stylistic rules that clash with Prettier formatting (.prettierrc)
  eslintConfigPrettier,
);
