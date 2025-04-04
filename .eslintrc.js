module.exports = {
    root: true,
    env: {
      node: true,
    },
    parser: 'vue-eslint-parser',
    parserOptions: {
      parser: '@babel/eslint-parser',
      requireConfigFile: false,
    },
    extends: [
      'eslint:recommended',
      'plugin:vue/vue3-essential',
    ],
    plugins: ['vue'],
    rules: {
      'no-unused-vars': 'warn',
      'vue/no-unused-components': 'warn',
    },
  };
  