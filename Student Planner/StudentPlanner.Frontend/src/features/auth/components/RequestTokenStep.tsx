import Input from '../../../components/common/Input';

interface RequestTokenStepProps {
  email: string;
}

export default function RequestTokenStep({ email }: RequestTokenStepProps) {
  return (
    <Input 
      type="email" 
      id="email" 
      label="University Email" 
      defaultValue={email} 
      required 
    />
  );
}
