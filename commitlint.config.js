const Configuration = {
  extends: ['@commitlint/config-conventional'],
  rules: {
    'body-max-line-length': [0, 'always'],
  },
};
module.exports = Configuration;