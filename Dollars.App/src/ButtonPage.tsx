import { useRef, useState } from 'react';

const ButtonPage = () => {

const [counter, setCounter] = useState(0);
const [isDisabled, setIsDisabled] = useState(false);
const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);

const onClick = () => {
    if(!isDisabled) {
        setIsDisabled(true);
        setCounter(p => p + 1);
        timeoutRef.current = setTimeout(reset, 3000);
    }    
};

const onReset = () => {    
    timeoutRef.current && clearTimeout(timeoutRef.current);
    reset();
}

const reset = () => {
    setIsDisabled(false);
}

return (
    <>
    <div>
        <button type="button" disabled={isDisabled} onClick={onClick}>Click{isDisabled ? ' - DISABLED' : ''}</button>
        {isDisabled && <button type="button" onClick={onReset}>Reset</button>}
    </div>
    <div>
        <h2>counter:</h2> {counter}
    </div>
    </>
);

}

export default ButtonPage;